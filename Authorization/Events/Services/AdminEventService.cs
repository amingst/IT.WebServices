﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using IT.WebServices.Authentication;
using IT.WebServices.Authorization.Events.Data;
using IT.WebServices.Authorization.Events.Extensions;
using IT.WebServices.Authorization.Events.Helpers;
using IT.WebServices.Fragments.Authorization.Events;
using IT.WebServices.Helpers;
using IT.WebServices.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace IT.WebServices.Authorization.Events.Services.Services
{
    [Authorize]
    public class AdminEventService : AdminEventInterface.AdminEventInterfaceBase
    {
        private readonly ILogger<EventService> _logger;
        private readonly IEventDataProvider _eventProvider;
        private readonly ITicketDataProvider _ticketDataProvider;
        private readonly ONUserHelper _userHelper;
        private readonly EventTicketClassHelper _ticketClassHelper;

        public AdminEventService(
            ILogger<EventService> logger,
            ITicketDataProvider ticketDataProvider,
            IEventDataProvider eventProvider,
            ONUserHelper userHelper,
            EventTicketClassHelper eventTicketClassHelper
        )
        {
            _logger = logger;
            _eventProvider = eventProvider;
            _ticketDataProvider = ticketDataProvider;
            _userHelper = userHelper;
            _ticketClassHelper = eventTicketClassHelper;
        }

        [Authorize(Roles = ONUser.ROLE_IS_EVENT_CREATOR_OR_HIGHER)]
        public override async Task<AdminCreateEventResponse> AdminCreateEvent(
            AdminCreateEventRequest request,
            ServerCallContext context
        )
        {
            var newEvent = new EventRecord() { EventId = Guid.NewGuid().ToString() };

            var now = Timestamp.FromDateTime(DateTime.UtcNow);

            newEvent.SinglePublic = new SingleEventPublicRecord()
            {
                EventId = newEvent.EventId,
                Title = request.Data.Title,
                Description = request.Data.Description,
                Venue = request.Data.Venue,
                StartOnUTC = request.Data.StartTimeUTC,
                EndOnUTC = request.Data.EndTimeUTC,
                CreatedOnUTC = now,
                ModifiedOnUTC = now,
            };

            newEvent.SinglePublic.Tags.AddRange(request.Data.Tags);
            newEvent.SinglePublic.TicketClasses.AddRange(request.Data.TicketClasses);

            newEvent.SinglePrivate = new();

            var success = await _eventProvider.Create(newEvent);
            if (!success)
            {
                return new AdminCreateEventResponse()
                {
                    Error = EventErrorExtensions.CreateError(EventErrorReason.CreateEventErrorUnknown, "An error occurred while creating event"),
                };
            }

            return new AdminCreateEventResponse()
            {
                Error = null, // Success case - no error
                Event = newEvent,
            };
        }

        [Authorize(Roles = ONUser.ROLE_IS_EVENT_CREATOR_OR_HIGHER)]
        public override async Task<AdminCreateEventResponse> AdminCreateRecurringEvent(
            AdminCreateRecurringEventRequest request,
            ServerCallContext context
        )
        {
            var response = new AdminCreateEventResponse();

            if (request == null || request.Data == null || request.RecurrenceRule == null)
            {
                response.Error = EventErrorExtensions.CreateInvalidRequestError("Missing Data or Recurrence Rule");
                return response;
            }

            var eventId = Guid.NewGuid();

            var combinedString = JsonSerializer.Serialize(
                new
                {
                    Frequency = request.RecurrenceRule.Frequency,
                    Interval = request.RecurrenceRule.Interval,
                    ByWeekday = request.RecurrenceRule.ByWeekday.OrderBy(d => d),
                    Count = request.RecurrenceRule.Count,
                    RepeatUntilUTC = request.RecurrenceRule.RepeatUntilUTC,
                    ExcludeDatesUTC = request.RecurrenceRule.ExcludeDatesUTC,
                    EventId = eventId,
                    VenueId = request.Data.Venue?.VenueId,
                }
            );

            string recurrenceHash = RecurrenceHelper.GenerateRecurrenceHash(combinedString);

            var userId = _userHelper.MyUserId; // Extension/middleware required
            var baseRecord = new EventRecord(request, userId.ToString(), recurrenceHash);
            // Expand the base into individual recurring records
            var instances = RecurrenceHelper.GenerateInstances(baseRecord);
            var records = new List<EventRecord>();

            foreach (var instance in instances)
            {
                var instanceId = Guid.NewGuid().ToString();
                records.Add(
                    new EventRecord
                    {
                        EventId = instanceId,
                        OneOfType = EventRecordOneOfType.EventOneOfRecurring,
                        RecurringPublic = baseRecord.RecurringPublic.Clone(), // Deep copy
                        RecurringPrivate = baseRecord.RecurringPrivate.Clone(),
                    }.Tap(rec =>
                    {
                        rec.EventId = instanceId;
                        rec.RecurringPublic.EventId = instanceId;
                        rec.RecurringPublic.TemplateStartOnUTC = Timestamp.FromDateTime(
                            instance.Start
                        );
                        rec.RecurringPublic.TemplateEndOnUTC = Timestamp.FromDateTime(instance.End);
                    })
                );
            }

            // Persist
            var result = await _eventProvider.CreateRecurring(records);

            if (!result)
            {
                response.Error = EventErrorExtensions.CreateError(EventErrorReason.CreateRecurringEventErrorUnknown, "Failed to persist recurring events");
                return response;
            }

            // Return the "template" event (first instance)
            response.Event = records.First();
            response.Error = null; // Success case - no error
            return response;
        }

        [Authorize(Roles = ONUser.ROLE_IS_EVENT_MODERATOR_OR_HIGHER)]
        public override async Task<AdminGetEventResponse> AdminGetEvent(
            AdminGetEventRequest request,
            ServerCallContext context
        )
        {
            Guid.TryParse(request.EventId, out var eventId);
            if (eventId == Guid.Empty)
                return new AdminGetEventResponse()
                {
                    Error = EventErrorExtensions.CreateInvalidRequestError("Invalid Event ID")
                };

            var found = await _eventProvider.GetById(eventId);
            return new AdminGetEventResponse() { Event = found };
        }

        [Authorize(Roles = ONUser.ROLE_IS_EVENT_MODERATOR_OR_HIGHER)]
        public override async Task<AdminGetEventsResponse> AdminGetEvents(
            AdminGetEventsRequest request,
            ServerCallContext context
        )
        {
            var res = new AdminGetEventsResponse();

            var enumerator = _eventProvider.GetEvents();

            switch (string.IsNullOrWhiteSpace(request.RecurrenceHash))
            {
                case true:
                    res.Events.AddRange(await GetSingleEvents(enumerator, request.IncludeCanceled));
                    break;
                case false:
                    res.Events.AddRange(
                        await GetRecurringEvents(
                            enumerator,
                            request.RecurrenceHash,
                            request.IncludeCanceled
                        )
                    );
                    break;
            }

            return res;
        }

        [Authorize(Roles = ONUser.ROLE_IS_EVENT_CREATOR_OR_HIGHER)]
        public override async Task<AdminModifyEventResponse> AdminModifyEvent(
            AdminModifyEventRequest request,
            ServerCallContext context
        )
        {
            var res = new AdminModifyEventResponse();

            if (!Guid.TryParse(request.EventId, out var eventId) || eventId == Guid.Empty)
            {
                res.Error = EventErrorExtensions.CreateInvalidRequestError("Invalid EventId passed");
                return res;
            }

            var existing = await _eventProvider.GetById(eventId);
            if (existing == null || existing.OneOfType != EventRecordOneOfType.EventOneOfSingle)
            {
                res.Error = EventErrorExtensions.CreateEventNotFoundError(eventId.ToString());
                return res;
            }

            var newData = request.Data;

            // Update fields
            var updated = existing.Clone();
            var single = updated.SinglePublic;
            single.Title = newData.Title;
            single.Description = newData.Description;
            single.Venue = newData.Venue;
            single.Location = "";
            single.StartOnUTC = newData.StartTimeUTC;
            single.EndOnUTC = newData.EndTimeUTC;
            single.Tags.Clear();
            single.Tags.AddRange(newData.Tags);
            single.TicketClasses.Clear();
            single.TicketClasses.AddRange(newData.TicketClasses);
            updated.SinglePublic = single;

            // Also update private metadata if it exists
            if (
                updated.OneOfType == EventRecordOneOfType.EventOneOfSingle
                && updated.SinglePrivate != null
            )
            {
                updated.SinglePrivate.ModifiedById =
                    context.GetHttpContext()?.User?.Identity?.Name ?? "unknown";
                updated.SinglePrivate.ExtraMetadata.Clear();
                if (newData.ExtraData != null)
                    updated.SinglePrivate.ExtraMetadata.Add(newData.ExtraData);
            }

            var success = await _eventProvider.Update(updated);
            if (!success)
            {
                res.Error = EventErrorExtensions.CreateError(EventErrorReason.CreateEventErrorUnknown, "Failed to update the event");
                return res;
            }

            res.Error = null; // Success case - no error

            return res;
        }

        [Authorize(Roles = ONUser.ROLE_IS_EVENT_CREATOR_OR_HIGHER)]
        public override async Task<AdminCancelEventResponse> AdminCancelEvent(
            AdminCancelEventRequest request,
            ServerCallContext context
        )
        {
            var res = new AdminCancelEventResponse();
            Guid.TryParse(request.EventId, out var eventId);
            if (eventId == Guid.Empty)
            {
                res.Error = EventErrorExtensions.CreateInvalidRequestError("Invalid Event ID");
                return res;
            }

            var rec = await _eventProvider.GetById(eventId);
            if (rec == null)
            {
                res.Error = EventErrorExtensions.CreateEventNotFoundError(eventId.ToString());
                return res;
            }
            var now = Timestamp.FromDateTime(DateTime.UtcNow);

            if (rec.OneOfType == EventRecordOneOfType.EventOneOfRecurring)
            {
                rec.RecurringPublic.CanceledOnUTC = now;
                rec.RecurringPublic.IsCanceled = true;
                rec.RecurringPrivate.CanceledForReason = request.Reason;
            }
            else if (rec.OneOfType == EventRecordOneOfType.EventOneOfSingle)
            {
                rec.SinglePublic.CanceledOnUTC = now;
                rec.SinglePublic.IsCanceled = true;
                rec.SinglePrivate.CanceledForReason = request.Reason;
            }
            else
            {
                res.Error = EventErrorExtensions.CreateError(EventErrorReason.CancelEventErrorUnknown, "Error canceling event");
                return res;
            }

            var success = await _eventProvider.Update(rec);

            if (!success)
            {
                res.Error = EventErrorExtensions.CreateError(EventErrorReason.CancelEventErrorUnknown, "Unknown error occurred while canceling event");
            }
            else
            {
                res.Error = null; // Success case - no error
            }

            return res;
        }

        [Authorize(Roles = ONUser.ROLE_IS_EVENT_CREATOR_OR_HIGHER)]
        public override async Task<AdminCancelAllRecurringEventsResponse> AdminCancelAllRecurringEvents(
            AdminCancelAllRecurringEventsRequest request,
            ServerCallContext context
        )
        {
            var response = new AdminCancelAllRecurringEventsResponse();

            if (string.IsNullOrWhiteSpace(request.RecurrenceHash))
            {
                response.Error = EventErrorExtensions.CreateInvalidHashError("RecurrenceHash is required");
                return response;
            }

            var utcNow = DateTime.UtcNow;
            var toCancel = new List<EventRecord>();

            try
            {
                await foreach (var record in _eventProvider.GetEvents())
                {
                    if (
                        record.OneOfType == EventRecordOneOfType.EventOneOfRecurring
                        && string.Equals(
                            record.RecurringPublic.RecurrenceHash,
                            request.RecurrenceHash,
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                    {
                        if (!record.RecurringPublic.IsCanceled)
                        {
                            // Mark canceled
                            var recurring = record.RecurringPublic;
                            recurring.IsCanceled = true;
                            recurring.CanceledOnUTC =
                                Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(utcNow);

                            toCancel.Add(record);
                        }
                    }
                }

                var updateResult = await _eventProvider.UpdateRecurring(toCancel);

                if (!updateResult)
                {
                    response.Error = EventErrorExtensions.CreateError(EventErrorReason.CreateRecurringEventErrorUnknown, "Failed to update recurring events during cancellation");
                    return response;
                }

                response.Error = null; // Success case - no error
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error cancelling recurring events with hash {RecurrenceHash}",
                    request.RecurrenceHash
                );
                response.Error = EventErrorExtensions.CreateError(EventErrorReason.CreateRecurringEventErrorUnknown, "Unexpected error occurred");
                return response;
            }
        }

        [Authorize(Roles = ONUser.ROLE_IS_EVENT_MODERATOR_OR_HIGHER)]
        public override async Task<AdminGetTicketResponse> AdminGetTicket(
            AdminGetTicketRequest request,
            ServerCallContext context
        )
        {
            Guid.TryParse(request.TicketId, out var ticketId);
            if (ticketId == Guid.Empty)
                return new AdminGetTicketResponse();

            Guid.TryParse(request.EventId, out var eventId);
            if (eventId == Guid.Empty)
                return new AdminGetTicketResponse();

            var found = await _ticketDataProvider.GetById(ticketId, eventId);
            return new AdminGetTicketResponse() { Record = found };
        }

        public override async Task<AdminGetTicketsForEventResponse> AdminGetTicketsForEvent(
            AdminGetTicketsForEventRequest request,
            ServerCallContext context
        )
        {
            Guid.TryParse(request.EventId, out var eventId);
            if (eventId == Guid.Empty)
                return new AdminGetTicketsForEventResponse();

            var found = await _ticketDataProvider.GetAllByEvent(eventId).ToList();
            var res = new AdminGetTicketsForEventResponse();
            res.Records.AddRange(found);

            return res;
        }

        [Authorize(Roles = ONUser.ROLE_IS_EVENT_MODERATOR_OR_HIGHER)]
        public override Task<AdminCancelOtherTicketResponse> AdminCancelOtherTicket(
            AdminCancelOtherTicketRequest request,
            ServerCallContext context
        )
        {
            throw new NotImplementedException();
        }

        [Authorize(Roles = ONUser.ROLE_IS_EVENT_MODERATOR_OR_HIGHER)]
        public override Task<AdminReserveEventTicketForUserResponse> AdminReserveEventTicketForUser(
            AdminReserveEventTicketForUserRequest request,
            ServerCallContext context
        )
        {
            return base.AdminReserveEventTicketForUser(request, context);
        }

        private async Task<List<EventRecord>> GetSingleEvents(
            IAsyncEnumerable<EventRecord> events,
            bool includeCanceled = false
        )
        {
            var res = new List<EventRecord>();
            await foreach (var item in events)
            {
                if (includeCanceled && item.SinglePublic.IsCanceled == true)
                {
                    res.Add(item);
                }

                if (!includeCanceled && !item.SinglePublic.IsCanceled)
                {
                    res.Add(item);
                }
            }

            return res;
        }

        private async Task<List<EventRecord>> GetRecurringEvents(
            IAsyncEnumerable<EventRecord> events,
            string recurrenceHash,
            bool includeCanceled = false
        )
        {
            var res = new List<EventRecord>();
            await foreach (var item in events)
            {
                // Only process recurring events with the matching recurrence hash
                if (item.OneOfType != EventRecordOneOfType.EventOneOfRecurring)
                    continue;

                if (
                    string.Equals(
                        item.RecurringPublic.RecurrenceHash,
                        recurrenceHash,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    if (includeCanceled && item.RecurringPublic.IsCanceled)
                    {
                        res.Add(item);
                    }
                    else if (!includeCanceled && !item.RecurringPublic.IsCanceled)
                    {
                        res.Add(item);
                    }
                }
            }

            return res;
        }
    }
}

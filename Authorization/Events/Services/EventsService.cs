using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using IT.WebServices.Authentication;
using IT.WebServices.Authorization.Events.Data;
using IT.WebServices.Authorization.Events.Helpers;
using IT.WebServices.Fragments.Authorization.Events;
using IT.WebServices.Helpers;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Utilities;

namespace IT.WebServices.Authorization.Events.Services
{
    public class EventsService : EventInterface.EventInterfaceBase
    {
        private readonly ILogger<EventsService> _logger;
        private readonly IEventDataProvider _eventProvider;
        private readonly ITicketDataProvider _ticketProvider;
        private readonly IRSVPDataProvider _rsvpProvider;
        private readonly IEventInstanceOverrideDataProvider _eventInstanceOverrideProvider;
        private readonly ONUserHelper _userHelper;

        public EventsService(
            ILogger<EventsService> logger,
            IEventDataProvider eventProvider,
            ITicketDataProvider ticketProvider,
            IRSVPDataProvider rsvpProvider,
            IEventInstanceOverrideDataProvider eventInstanceOverrideProvider,
            ONUserHelper userHelper
        )
        {
            _logger = logger;
            _eventProvider = eventProvider;
            _ticketProvider = ticketProvider;
            _rsvpProvider = rsvpProvider;
            _eventInstanceOverrideProvider = eventInstanceOverrideProvider;
            _userHelper = userHelper;
        }

        public override async Task<CreateEventResponse> CreateEvent(
            CreateEventRequest request,
            ServerCallContext context
        )
        {
            var newGuid = Guid.NewGuid();
            var now = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow);
            var userToken = ONUserHelper.ParseUser(context.GetHttpContext());
            var newEvent = new EventRecord()
            {
                EventId = newGuid.ToString(),
                Public = new()
                {
                    Title = request.Title,
                    Description = request.Description,
                    Type = request.Type,
                    AccessData = request.AccessData,
                    LocationData = request.LocationData,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    LifecycleMetadata = new() { CreatedOnUTC = now, ModifiedOnUTC = now },
                    MaxAttendees = request.MaxAttendees,
                },
                Private = new()
                {
                    LifecycleMetadata = new()
                    {
                        CreatedById = (userToken?.Id ?? newGuid).ToString(),
                        ModifiedById = (userToken?.Id ?? newGuid).ToString(),
                    },
                },
            };

            if (request.RecurrenceRule != null)
            {
                newEvent.Public.Recurrence = request.RecurrenceRule;
            }

            var res = await _eventProvider.Create(newEvent);
            if (!res)
                return new CreateEventResponse
                {
                    ErrorType = EventErrorType.Unknown,
                    Message = "Unknown Error",
                };

            return new CreateEventResponse
            {
                ErrorType = EventErrorType.NoError,
                Message = "Success",
                Record = newEvent,
            };
        }

        public override async Task<GetEventByIdResponse> GetEventById(
            GetEventByIdRequest request,
            ServerCallContext context
        )
        {
            Guid.TryParse(request.EventId, out var eventGuid);
            if (eventGuid == Guid.Empty)
            {
                return new GetEventByIdResponse
                {
                    ErrorType = EventErrorType.NotFound,
                    Message = "Event Not Found",
                };
            }

            var found = await _eventProvider.GetById(eventGuid);

            return new GetEventByIdResponse()
            {
                ErrorType = EventErrorType.NoError,
                Record = found,
            };
        }

        public override async Task<GetEventsResponse> GetEvents(
            GetEventsRequest request,
            ServerCallContext context
        )
        {
            var found = await _eventProvider.GetAll().ToList();
            var res = new GetEventsResponse { PageTotalItems = (uint)found.Count() };

            if (found.Count > 0)
            {
                res.Records.AddRange(found);
            }

            return res;
        }

        public override async Task<ModifyEventResponse> ModifyEvent(
            ModifyEventRequest request,
            ServerCallContext context
        )
        {
            Guid.TryParse(request.EventId, out var eventGuid);
            if (eventGuid == Guid.Empty)
            {
                return new ModifyEventResponse()
                {
                    ErrorType = EventErrorType.NullRequestData,
                    Message = "Invalid Id Passed",
                };
            }

            var found = await _eventProvider.GetById(eventGuid);
            if (found == null)
            {
                return new ModifyEventResponse()
                {
                    ErrorType = EventErrorType.NotFound,
                    Message = "Event Not Found",
                };
            }

            var now = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow);
            var userToken = ONUserHelper.ParseUser(context.GetHttpContext());
            found.Public.Type = request.Type;
            found.Public.AccessData = request.AccessData;
            found.Public.LocationData = request.LocationData;
            found.Public.StartDate = request.StartDate;
            found.Public.EndDate = request.EndDate;
            found.Public.MaxAttendees = request.MaxAttendees;
            found.Public.LifecycleMetadata.ModifiedOnUTC = now;
            found.Private.LifecycleMetadata.ModifiedById = (userToken?.Id).ToString();

            await _eventProvider.Save(found);
            return new ModifyEventResponse()
            {
                ErrorType = EventErrorType.NoError,
                Message = "Success",
            };
        }

        public override async Task<DeleteEventResponse> DeleteEvent(
            DeleteEventRequest request,
            ServerCallContext context
        )
        {
            Guid.TryParse(request.EventId, out var eventGuid);
            if (eventGuid == Guid.Empty)
            {
                return new DeleteEventResponse
                {
                    ErrorType = EventErrorType.NullRequestData,
                    Message = "Invalid Id",
                };
            }

            var res = await _eventProvider.Delete(eventGuid);
            if (!res)
            {
                return new DeleteEventResponse
                {
                    ErrorType = EventErrorType.Unknown,
                    Message = "Failed To Delete",
                };
            }

            return new DeleteEventResponse()
            {
                ErrorType = EventErrorType.NoError,
                Message = "Success",
            };
        }

        public override async Task<CreateTicketResponse> CreateTicket(
            CreateTicketRequest request,
            ServerCallContext context
        )
        {
            Guid.TryParse(request.EventId, out var eventGuid);
            if (eventGuid == Guid.Empty)
                return new CreateTicketResponse()
                {
                    Error = EventErrorType.NotFound,
                    Message = "Event Does Not Exist",
                };

            var ticketId = Guid.NewGuid();
            var now = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow);
            var userToken = ONUserHelper.ParseUser(context.GetHttpContext());
            var newTicket = new EventTicketRecord()
            {
                TicketId = ticketId.ToString(),
                Public = new EventTicketPublicRecord()
                {
                    EventId = eventGuid.ToString(),
                    TicketName = request.TicketName,
                    MaxAttendees = request.MaxAttendees,
                    MaxPerUser = request.MaxPerUser,
                    Price = request.Price,
                },
                Private = new EventTicketPrivateRecord()
                {
                    SaleStartOnUTC = request.SaleStartOnUTC,
                    SaleEndOnUTC = request.SaleEndOnUTC,
                    QuantityAvailible = request.QuantityAvailable,
                },
            };

            var res = await _ticketProvider.Create(newTicket);
            if (!res)
            {
                return new CreateTicketResponse()
                {
                    Error = EventErrorType.Unknown,
                    Message = "Failed To Create Ticket",
                };
            }

            return new CreateTicketResponse()
            {
                Error = EventErrorType.NoError,
                Message = "Created Ticket",
                Record = newTicket,
            };
        }

        public override async Task<GetTicketResponse> GetTicket(
            GetTicketRequest request,
            ServerCallContext context
        )
        {
            Guid.TryParse(request.TicketId, out var ticketId);
            if (ticketId == Guid.Empty)
            {
                return new GetTicketResponse()
                {
                    ErrorType = EventErrorType.NullRequestData,
                    Message = "Invalid Ticket Id Passed",
                };
            }

            var res = await _ticketProvider.GetById(ticketId);
            if (res == null)
            {
                return new GetTicketResponse()
                {
                    ErrorType = EventErrorType.NotFound,
                    Message = "Ticket Not Found",
                };
            }

            return new GetTicketResponse() { ErrorType = EventErrorType.NoError, Record = res };
        }

        public override async Task<GetTicketsResposne> GetTickets(
            GetTicketsRequest request,
            ServerCallContext context
        )
        {
            var res = new GetTicketsResposne();
            var found = await _ticketProvider.GetAll().ToList();
            res.PageTotalItems = (uint)found.Count();

            if (found.Count() > 0)
                res.Records.AddRange(found);

            return res;
        }

        public override async Task<GetTicketsByEventResponse> GetTicketsByEvent(
            GetTicketsByEventRequest request,
            ServerCallContext context
        )
        {
            var res = new GetTicketsByEventResponse();
            Guid.TryParse(request.EventId, out var eventId);
            if (eventId == Guid.Empty)
                return res;

            var found = await _ticketProvider.GetAllByEvent(eventId).ToList();
            if (found.Count > 0)
            {
                res.PageTotalItems = (uint)found.Count();
                res.Records.AddRange(found);
            }

            return res;
        }

        public override async Task<CreateEventRSVPResponse> CreateEventRSVP(
            CreateEventRSVPRequest request,
            ServerCallContext context
        )
        {
            var res = new CreateEventRSVPResponse();
            Guid.TryParse(request.EventId, out Guid eventGuid);
            if (eventGuid == Guid.Empty)
            {
                res.ErrorType = EventErrorType.Unknown;
                res.Message = "Invalid Event Guid";
                return res;
            }

            if (!_eventProvider.Exists(eventGuid).Result)
            {
                res.ErrorType = EventErrorType.NotFound;
                res.Message = "Event Not Found";
                return res;
            }

            Guid.TryParse(request.TicketId, out var ticketGuid);
            if (ticketGuid == Guid.Empty)
            {
                res.ErrorType = EventErrorType.Unknown;
                res.Message = "Invalid Ticket Guid";
                return res;
            }

            if (!_ticketProvider.Exists(eventGuid, ticketGuid).Result)
            {
                res.ErrorType = EventErrorType.NotFound;
                res.Message = "Ticket Not Found";
                return res;
            }

            Guid.TryParse(request.UserId, out var userGuid);
            if (userGuid == Guid.Empty)
            {
                res.ErrorType = EventErrorType.Unknown;
                res.Message = "Invalid User Guid";
                return res;
            }

            var id = Guid.NewGuid();
            var now = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow);
            var record = new EventRSVPRecord()
            {
                EventRSVPId = id.ToString(),
                EventId = eventGuid.ToString(),
                TicketId = ticketGuid.ToString(),
                UserId = userGuid.ToString(),
                TotalPriceCents = request.TotalPriceCents,
                AmountReserved = request.AmountReserved,
                CreatedOnUTC = now,
            };
            var created = await _rsvpProvider.Create(record);
            if (!created)
            {
                res.ErrorType = EventErrorType.Unknown;
                res.Message = "Failed To Create RSVP";
                return res;
            }

            res.ErrorType = EventErrorType.NoError;
            res.Message = "Successfully Created RSVP";
            res.Record = record;
            return res;
        }

        public override async Task<CancelEventRSVPResponse> CancelEventRSVP(
            CancelEventRSVPRequest request,
            ServerCallContext context
        )
        {
            Guid.TryParse(request.EventRSVPId, out var rsvpGuid);
            if (rsvpGuid == Guid.Empty)
            {
                return new CancelEventRSVPResponse()
                {
                    ErrorType = EventErrorType.Unknown,
                    Message = "Invalid Event RSVP Guid",
                };
            }

            var found = await _rsvpProvider.GetById(rsvpGuid);
            if (found is null)
            {
                return new CancelEventRSVPResponse()
                {
                    ErrorType = EventErrorType.NotFound,
                    Message = "Reservation Could Not Be Found",
                };
            }

            found.CancelledOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(
                DateTime.UtcNow
            );
            await _rsvpProvider.Save(found);

            return new CancelEventRSVPResponse()
            {
                ErrorType = EventErrorType.NoError,
                Message = "Reservation Cancelled",
            };
        }

        public override async Task<GetAllRSVPResponse> GetRSVPForEvent(
            GetRSVPForEventRequest request,
            ServerCallContext context
        )
        {
            var res = new GetAllRSVPResponse();
            Guid.TryParse(request.EventId, out var eventGuid);
            if (eventGuid == Guid.Empty)
                return res;

            var found = await _rsvpProvider.GetAllByEvent(eventGuid).ToList();
            if (found.Count > 0)
            {
                res.Records.AddRange(found);
                res.PageTotalItems = (uint)found.Count;
            }
            return res;
        }

        public override async Task<GetAllRSVPResponse> GetRSVPForUser(
            GetRSVPForUserRequest request,
            ServerCallContext context
        )
        {
            var res = new GetAllRSVPResponse();
            Guid.TryParse(request.UserId, out var userGuid);
            if (userGuid == Guid.Empty)
            {
                return res;
            }

            var found = await _rsvpProvider.GetAllByUser(userGuid).ToList();
            if (found.Count > 0)
            {
                res.PageTotalItems = (uint)found.Count;
                res.Records.AddRange(found);
            }
            return res;
        }

        public override async Task<GetRSVPByIdResponse> GetRSVPById(
            GetRSVPByIdRequest request,
            ServerCallContext context
        )
        {
            var res = new GetRSVPByIdResponse();
            Guid.TryParse(request.EventRSVPId, out var eventRsvpGuid);
            if (eventRsvpGuid == Guid.Empty)
            {
                res.ErrorType = EventErrorType.Unknown;
                res.Message = "Invalid RSVP Guid";
                return res;
            }

            var found = await _rsvpProvider.GetById(eventRsvpGuid);
            if (found is null)
            {
                res.ErrorType = EventErrorType.NotFound;
                res.Message = "RSVP Does Not Exist";
                return res;
            }

            res.ErrorType = EventErrorType.NoError;
            res.Record = found;
            return res;
        }

        public override async Task<GetInstancesForEventResponse> GetInstancesForEvent(
            GetInstancesForEventRequest request,
            ServerCallContext context
        )
        {
            var res = new GetInstancesForEventResponse();

            if (!Guid.TryParse(request.EventId, out var eventId) || eventId == Guid.Empty)
                return res;

            var foundEvent = await _eventProvider.GetById(eventId);
            if (foundEvent is null)
                return res;

            // Pagination parameters from the request (assuming zero-based index for start)
            var pageOffsetStart = (int)request.PageOffsetStart;
            var pageOffsetEnd = (int)request.PageOffsetEnd;

            if (!foundEvent.IsRecurring)
            {
                if (pageOffsetStart > 0)
                {
                    // If start offset is beyond available single instance, return empty
                    return res;
                }

                res.Instances.Add(
                    new EventInstance
                    {
                        ParentEventId = foundEvent.EventId,
                        StartDate = foundEvent.Public.StartDate,
                        EndDate = foundEvent.Public.EndDate,
                        IsCancelled = foundEvent.Public.LifecycleMetadata?.CanceledOnUTC != null,
                    }
                );

                res.PageTotalItems = 1;
                res.PageOffsetStart = 0;
                res.PageOffsetEnd = 0;
                return res;
            }

            var generatedInstances = EventInstanceHelper
                .BuildInstancesForEvent(foundEvent)
                .ToDictionary(x => x.InstanceId);

            await foreach (
                var overrideRecord in _eventInstanceOverrideProvider.GetByEventId(eventId)
            )
            {
                if (!string.IsNullOrWhiteSpace(overrideRecord.InstanceId))
                {
                    generatedInstances[overrideRecord.InstanceId] = new EventInstance
                    {
                        InstanceId = overrideRecord.InstanceId,
                        ParentEventId = overrideRecord.ParentEventId,
                        StartDate = overrideRecord.StartDate,
                        EndDate = overrideRecord.EndDate,
                        IsCancelled = overrideRecord.IsCanceled,
                    };
                }
            }

            var allInstancesOrdered = generatedInstances.Values.OrderBy(i => i.StartDate).ToList();
            var totalCount = allInstancesOrdered.Count;

            // Clamp offsets
            if (pageOffsetStart < 0)
                pageOffsetStart = 0;

            if (pageOffsetEnd < pageOffsetStart || pageOffsetEnd >= totalCount)
                pageOffsetEnd = totalCount - 1;

            var countToTake = (pageOffsetEnd - pageOffsetStart) + 1;
            if (countToTake < 0)
                countToTake = 0;

            var pagedInstances = allInstancesOrdered.Skip(pageOffsetStart).Take(countToTake);

            res.Instances.AddRange(pagedInstances);
            res.PageTotalItems = (uint)totalCount;
            res.PageOffsetStart = (uint)pageOffsetStart;
            res.PageOffsetEnd = (uint)(pageOffsetStart + pagedInstances.Count() - 1);

            return res;
        }

        public override async Task<OverrideEventInstanceResponse> OverrideEventInstance(
            OverrideEventInstanceRequest request,
            ServerCallContext context
        )
        {
            var res = new OverrideEventInstanceResponse();
            Guid.TryParse(request.EventId, out var eventId);
            if (eventId == Guid.Empty)
            {
                res.Success = false;
                res.Error = "Invalid EventID passed";
                return res;
            }

            var success = await _eventInstanceOverrideProvider.Create(request.OverrideData);

            res.Success = success;
            return res;
        }
    }
}

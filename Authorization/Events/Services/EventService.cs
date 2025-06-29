using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using IT.WebServices.Authentication;
using IT.WebServices.Authorization.Events.Data;
using IT.WebServices.Authorization.Events.Extensions;
using IT.WebServices.Authorization.Events.Helpers;
using IT.WebServices.Fragments.Authorization.Events;
using IT.WebServices.Helpers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace IT.WebServices.Authorization.Events.Services
{
    public class EventService : EventInterface.EventInterfaceBase
    {
        private readonly ILogger<EventService> _logger;
        private readonly IEventDataProvider _eventProvider;
        private readonly ITicketDataProvider _ticketProvider;
        private readonly ONUserHelper _userHelper;

        public EventService(
            ILogger<EventService> logger,
            IEventDataProvider eventProvider,
            ITicketDataProvider ticketProvider,
            ONUserHelper userHelper
        )
        {
            _logger = logger;
            _eventProvider = eventProvider;
            _ticketProvider = ticketProvider;
            _userHelper = userHelper;
        }

        public override async Task<GetEventResponse> GetEvent(GetEventRequest request, ServerCallContext context)
        {
            Guid.TryParse(request.EventId, out var eventId);
            if (eventId != Guid.Empty)
                return new GetEventResponse()
                {
                    Error = new()
                    {
                        GetEventError = GetEventErrorType.GetEventUnknown,
                        Message = "Invalid Id",
                    },
                };

            var found = await _eventProvider.GetById(eventId);
            var rec = found.Item1;
            if (found.Item2 != GetEventErrorType.GetEventNoError)
            {
                return new GetEventResponse()
                {
                    Error = new()
                    {
                        GetEventError = found.Item2,
                        Message = "Error Getting Event",
                    },
                };
            }

            if (rec == null)
            {
                return new GetEventResponse()
                {
                    Error = new()
                    {
                        GetEventError = GetEventErrorType.GetEventNotFound,
                        Message = "Event Not Found",
                    },
                };
            }

            var res = new GetEventResponse()
            {
                Event = new EventPublicRecord()
                {
                    EventId = rec.EventId,
                },
                Error = new EventError()
                {
                    GetEventError = GetEventErrorType.GetEventNoError,
                    Message = "Success",
                },
            };

            switch (rec.OneOfType)
            {
                case EventRecordOneOfType.EventOneOfSingle:
                    res.Event.SinglePublic = rec.SinglePublic;
                    break;
                case EventRecordOneOfType.EventOneOfRecurring:
                    res.Event.RecurringPublic = rec.RecurringPublic;
                    break;
                default:
                    res.Error.GetEventError = GetEventErrorType.GetEventUnknown;
                    res.Error.Message = "Unknown Event Type";
                    break;
            }

            return res;
        }

        public override async Task<GetEventsResponse> GetEvents(GetEventsRequest request, ServerCallContext context)
        {
            var res = new GetEventsResponse();
            var enumerator = _eventProvider.GetEvents();

            // TODO: Make this more efficient
            await foreach (var item in enumerator)
            {
                switch (item.OneOfType)
                {
                     case EventRecordOneOfType.EventOneOfSingle:
                        res.Events.Add(new EventPublicRecord()
                        {
                            EventId = item.EventId,
                            SinglePublic = item.SinglePublic,
                        });
                        break;
                    case EventRecordOneOfType.EventOneOfRecurring:
                        res.Events.Add(new EventPublicRecord()
                        {
                            EventId = item.EventId,
                            RecurringPublic = item.RecurringPublic,
                        });
                        break;
                    default:
                        _logger.LogWarning("Unknown event type encountered: {Type}", item.OneOfType);
                        break;
                }
            }

            res.Error = new EventError()
            {
                GetEventError = GetEventErrorType.GetEventNoError,
                Message = "Success",
            };
            return res;
        }

        public override async Task<GetOwnTicketResponse> GetOwnTicket(GetOwnTicketRequest request, ServerCallContext context)
        {
            var res = new GetOwnTicketResponse();
            var user = ONUserHelper.ParseUser(context.GetHttpContext());

            return res;
        }

        public override async Task<GetOwnTicketsResponse> GetOwnTickets(GetOwnTicketsRequest request, ServerCallContext context)
        {
            var user = ONUserHelper.ParseUser(context.GetHttpContext());

            return new GetOwnTicketsResponse();
        }

        public override async Task<CancelOwnTicketResponse> CancelOwnTicket(CancelOwnTicketRequest request, ServerCallContext context)
        {
            var user = ONUserHelper.ParseUser(context.GetHttpContext());

            return new CancelOwnTicketResponse();
        }

        public override async Task<ReserveTicketForEventResponse> ReserveTicketForEvent(ReserveTicketForEventRequest request, ServerCallContext context)
        {
            var res = new ReserveTicketForEventResponse();
            var user = ONUserHelper.ParseUser(context.GetHttpContext());
            if (user == null)
            {
                res.Error = new TicketError()
                {
                    ReserveTicketError = ReserveTicketErrorType.ReserveTicketUnauthorized,
                    Message = "User not authorized",
                };
                return res;
            }

            Guid.TryParse(request.EventId, out var eventId);
            if (eventId == Guid.Empty)
            {
                res.Error = new TicketError()
                {
                    ReserveTicketError = ReserveTicketErrorType.ReserveTicketInvalidRequest,
                    Message = "Invalid Event Id",
                };
                return res;
            }

            var (eventRecord, eventError) = await _eventProvider.GetById(eventId);
            if (eventRecord == null || eventError != GetEventErrorType.GetEventNoError)
            {
                res.Error = new TicketError()
                {
                    ReserveTicketError = ReserveTicketErrorType.ReserveTicketEventNotFound,
                    Message = "Event not found",
                };
                return res;
            }

            var ticketClass = GetTicketClassFromEventRecord(eventRecord, request.TicketClassId);
            if (ticketClass == null)
            {
                res.Error = new TicketError()
                {
                    ReserveTicketError = ReserveTicketErrorType.ReserveTicketInvalidRequest,
                    Message = "Invalid Ticket Class Id",
                };
                return res;
            }

            // TODO: Refactor this to EventTicketClass Extension Method
            var amountAvailable = (int)ticketClass.AmountAvailible;
            var numToReserve = (int)request.Quantity;
            var maxPerUser = (int)ticketClass.MaxTicketsPerUser;
            if ( amountAvailable<= 0 || amountAvailable - numToReserve <= 0)
            {
                res.Error = new TicketError()
                {
                    ReserveTicketError = ReserveTicketErrorType.ReserveTicketInvalidRequest,
                    Message = "No tickets available for this class",
                };
                return res;
            }

            if (numToReserve > maxPerUser)
            {
                res.Error = new TicketError()
                {
                    ReserveTicketError = ReserveTicketErrorType.ReserveTicketMaxLimitReached,
                    Message = $"You can only reserve {maxPerUser} tickets per user",
                };
                return res;
            }

            // TODO: Validate User Hasn't already bought the limit and if the limit isn't reached, will the reservation put the user over the limit

            // TODO: Refactor this to EventTicketClass Extension Method
            var now = Timestamp.FromDateTime(DateTime.UtcNow);
            if (now < ticketClass.SaleStartOnUTC || now > ticketClass.SaleEndOnUTC)
            {
                res.Error = new TicketError()
                {
                    ReserveTicketError = ReserveTicketErrorType.ReserveTicketNotOnSale,
                    Message = "Tickets are not on sale at this time",
                };
                return res;
            }

            var ticketsToReserve = GenerateTicketRecords(numToReserve, eventRecord, user.Id.ToString(), ticketClass);
            if (ticketsToReserve.Count == 0)
            {
                res.Error = new TicketError()
                {
                    ReserveTicketError = ReserveTicketErrorType.ReserveTicketUnknown,
                    Message = "Unknown Error Has Occured"
                };
                return res;
            }

            var success = await _ticketProvider.Create(ticketsToReserve);
            if (!success)
            {
                res.Error = new TicketError()
                {
                    ReserveTicketError = ReserveTicketErrorType.ReserveTicketUnknown,
                    Message = "Unknown Error Has Occured"
                };
                return res;
            }

            res.Tickets.AddRange(ticketsToReserve);
            res.Error = new TicketError()
            {
                ReserveTicketError = ReserveTicketErrorType.ReserveTicketNoError,
                Message = "Success",
            };
            return res;
        }

        public override Task<UseTicketResponse> UseTicket(UseTicketRequest request, ServerCallContext context)
        {
            return base.UseTicket(request, context);
        }

        private EventTicketClass GetTicketClassFromEventRecord(Fragments.Authorization.Events.EventRecord record, string ticketClassId)
        {
            EventTicketClass ticketClass = null;
            if (record.OneOfType == EventRecordOneOfType.EventOneOfSingle)
            {
                ticketClass = record.SinglePublic.TicketClasses.FirstOrDefault(tc => tc.TicketClassId ==ticketClassId);
            }
            else if (record.OneOfType == EventRecordOneOfType.EventOneOfRecurring)
            {
                ticketClass = record.RecurringPublic.TicketClasses.FirstOrDefault(tc => tc.TicketClassId ==ticketClassId);
            }

            return ticketClass;
        }

        // TODO: Refactor this to EventTicketRecord Extension Method
        private List<EventTicketRecord> GenerateTicketRecords(int numToGenerate, Fragments.Authorization.Events.EventRecord eventRecord, string userId, EventTicketClass ticketClass)
        {
            List<EventTicketRecord> tickets = new List<EventTicketRecord>();

            for (int i = 0; i <= numToGenerate; i++)
            {
                var now = Timestamp.FromDateTime(DateTime.UtcNow);
                var ticket = new EventTicketRecord()
                {
                    TicketId = Guid.NewGuid().ToString(),
                    Public = new EventTicketPublicRecord()
                    {
                        TicketClassId = ticketClass.TicketClassId,
                        Title = ticketClass.Name + " " + ( eventRecord.EventPublicRecordOneOfCase == Fragments.Authorization.Events.EventRecord.EventPublicRecordOneOfOneofCase.SinglePublic ? eventRecord.SinglePublic.Title : eventRecord.RecurringPublic.Title),
                        EventId = eventRecord.EventId,
                        Status = EventTicketStatus.TicketStatusAvailable,
                        CreatedOnUTC = now,
                        ModifiedOnUTC = now,
                        ExpiredOnUTC = eventRecord.EventPublicRecordOneOfCase == Fragments.Authorization.Events.EventRecord.EventPublicRecordOneOfOneofCase.SinglePublic
                            ? eventRecord.SinglePublic.EndOnUTC
                            : eventRecord.RecurringPublic.TemplateEndOnUTC,
                    },
                    Private = new EventTicketPrivateRecord()
                    {
                        UserId = userId,
                        CreatedById = userId,
                        ModifiedById = userId,
                    },
                };

                tickets.Add(ticket);
            }

            return tickets;
        }
    }
}

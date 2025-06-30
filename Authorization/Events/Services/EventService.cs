using Google.Protobuf.Collections;
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
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace IT.WebServices.Authorization.Events.Services
{
    [Authorize]
    public class EventService : EventInterface.EventInterfaceBase
    {
        private readonly ILogger<EventService> _logger;
        private readonly IEventDataProvider _eventProvider;
        private readonly ITicketDataProvider _ticketProvider;
        private readonly ONUserHelper _userHelper;
        private readonly EventTicketClassHelper _ticketClassHelper;

        public EventService(
            ILogger<EventService> logger,
            IEventDataProvider eventProvider,
            ITicketDataProvider ticketProvider,
            ONUserHelper userHelper,
            EventTicketClassHelper ticketClassHelper
        )
        {
            _logger = logger;
            _eventProvider = eventProvider;
            _ticketProvider = ticketProvider;
            _userHelper = userHelper;
            _ticketClassHelper = ticketClassHelper;
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

            return  new GetEventResponse()
            {
                Event = rec.GetPublicRecord(),
                Error = new EventError()
                {
                    GetEventError = GetEventErrorType.GetEventNoError,
                    Message = "Success",
                },
            };
        }

        public override async Task<GetEventsResponse> GetEvents(GetEventsRequest request, ServerCallContext context)
        {
            var classes = _ticketClassHelper.GetAll();
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

            var tickets = await _ticketProvider.GetAllByUser(user.Id).ToList();
            var foundTicket = tickets.FirstOrDefault(t => t.TicketId == request.TicketId);

            res.Record = foundTicket.Public;
            return res;
        }

        public override async Task<GetOwnTicketsResponse> GetOwnTickets(GetOwnTicketsRequest request, ServerCallContext context)
        {
            var user = ONUserHelper.ParseUser(context.GetHttpContext());
            var tickets = await _ticketProvider.GetAllByUser(user.Id).ToList();
            var res  = new GetOwnTicketsResponse();
            res.Records.AddRange(tickets.Select(t => t.Public));
            return res;
        }

        public override async Task<CancelOwnTicketResponse> CancelOwnTicket(CancelOwnTicketRequest request, ServerCallContext context)
        {
            var user = ONUserHelper.ParseUser(context.GetHttpContext());
            var res = new CancelOwnTicketResponse();

            var tickets = await _ticketProvider.GetAllByUser(user.Id).ToList();
            var foundTicket = tickets.FirstOrDefault(t => t.TicketId == request.TicketId);
            if (foundTicket == null)
            {
                res.Error = new TicketError()
                {
                    CancelTicketError = CancelTicketErrorType.CancelTicketTicketNotFound,
                    Message = "Ticket not found",
                };
                return res;
            }

            foundTicket = foundTicket.Cancel(user.Id.ToString());

           var success = await _ticketProvider.Update(foundTicket);
            if (!success)
            {
                res.Error = new TicketError()
                {
                    CancelTicketError = CancelTicketErrorType.CancelTicketTicketNotFound,
                    Message = "Unknown Error Has Occured",
                };
                return res;
            }
            res.Error = new TicketError()
            {
                CancelTicketError = CancelTicketErrorType.CancelTicketNoError,
                Message = "Success",
            };
            return res;
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

            var ticketClass = _ticketClassHelper.GetById(request.TicketClassId);
            if (ticketClass == null)
            {
                res.Error = new TicketError()
                {
                    ReserveTicketError = ReserveTicketErrorType.ReserveTicketInvalidRequest,
                    Message = "Invalid Ticket Class Id",
                };
                return res;
            }

            if (!ticketClass.HasRequestedAmount((int) request.Quantity))
            {
                res.Error = new TicketError()
                {
                    ReserveTicketError = ReserveTicketErrorType.ReserveTicketInvalidRequest,
                    Message = "No tickets available for this class",
                };
                return res;
            }

            // TODO: Count Tickets Reserved by User For This Event And Pass Into HitReservationLimit Second Parameter
            if (ticketClass.HitReservationLimit((int)request.Quantity))
            {
                res.Error = new TicketError()
                {
                    ReserveTicketError = ReserveTicketErrorType.ReserveTicketMaxLimitReached,
                    Message = $"You can only reserve {ticketClass.MaxTicketsPerUser} tickets per user",
                };
                return res;
            }

            if (!ticketClass.IsOnSale())
            {
                res.Error = new TicketError()
                {
                    ReserveTicketError = ReserveTicketErrorType.ReserveTicketNotOnSale,
                    Message = "Tickets are not on sale at this time",
                };
                return res;
            }

            var ticketsToReserve = EventTicketRecord.GenerateRecords((int) request.Quantity, eventRecord, user.Id.ToString(), ticketClass);
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

        public override async Task<UseTicketResponse> UseTicket(UseTicketRequest request, ServerCallContext context)
        {
            var user = ONUserHelper.ParseUser(context.GetHttpContext());
            var res = new UseTicketResponse();  
            var tickets = await _ticketProvider.GetAllByUser(user.Id).ToList();
            var foundTicket = tickets.FirstOrDefault(t => t.TicketId == request.TicketId);

            if (foundTicket == null)
            {
                res.Error = new TicketError()
                {
                    UseTicketError = UseTicketErrorType.UseTicketTicketNotFound,
                    Message = "Ticket not found",
                };
                return res;
            }

            if (foundTicket.Public.Status == EventTicketStatus.TicketStatusUsed)
            {
                res.Error = new TicketError()
                {
                    UseTicketError = UseTicketErrorType.UseTicketAlreadyUsed,
                    Message = "Ticket is not available for use",
                };
                return res;
            }

            if (foundTicket.Public.Status == EventTicketStatus.TicketStatusCanceled)
            {
                res.Error = new TicketError()
                {
                    UseTicketError = UseTicketErrorType.UseTicketCanceled,
                    Message = "Ticket is canceled and cannot be used",
                };
                return res;
            }

            if (foundTicket.Public.Status == EventTicketStatus.TicketStatusExpired)
            {
                res.Error = new TicketError()
                {
                    UseTicketError = UseTicketErrorType.UseTicketExpired,
                    Message = "Ticket is expired and cannot be used",
                };
                return res;
            }

            foundTicket = foundTicket.MarkAsUsed(user.Id.ToString());

            var success = await _ticketProvider.Update(foundTicket);
            if (!success)
            {
                res.Error = new TicketError()
                {
                    UseTicketError = UseTicketErrorType.UseTicketUnknown,
                    Message = "Unknown Error Has Occured",
                };
                return res;
            }

            res.Error = new TicketError()
            {
                UseTicketError = UseTicketErrorType.UseTicketNoError,
                Message = "Success",
            };
            return res;
        }
    }
}

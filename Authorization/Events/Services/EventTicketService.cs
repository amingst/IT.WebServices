using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using IT.WebServices.Authorization.Events.Data;
using IT.WebServices.Fragments.Authorization.Events;
using IT.WebServices.Helpers;
using Microsoft.Extensions.Logging;

namespace IT.WebServices.Authorization.Events.Services
{
    public class EventTicketService : EventTicketInterface.EventTicketInterfaceBase
    {
        private readonly ILogger<EventTicketService> _logger;
        private readonly ITicketDataProvider _ticketDataProvider;

        public EventTicketService(
            ILogger<EventTicketService> logger,
            ITicketDataProvider ticketDataProvider
        )
        {
            _logger = logger;
            _ticketDataProvider = ticketDataProvider;
        }

        // TODO: Validate User Against Ticket Class
        // TODO: Validate Ticket Class
        public override async Task<CreateTicketResponse> CreateTicket(
            CreateTicketRequest request,
            ServerCallContext context
        )
        {
            Guid.TryParse(request.TicketClassId, out var ticketClassId);
            if (ticketClassId == Guid.Empty)
                return new CreateTicketResponse()
                {
                    Error = TicketsCreateErrorType.CreateTicketInvalidRequest,
                    Message = "Invalid Ticket Class Id",
                };

            var now = Timestamp.FromDateTime(DateTime.UtcNow);

            var newTicket = new EventTicketRecord()
            {
                TicketId = Guid.NewGuid().ToString(),
                Public = new EventTicketPublicRecord()
                {
                    TicketClassId = ticketClassId.ToString(),
                    Title = request.Title,
                    EventId = request.EventId,
                    CreatedOnUTC = now,
                    ModifiedOnUTC = now,
                },
                Private = new EventTicketPrivateRecord(),
            };

            var success = await _ticketDataProvider.Create(newTicket);
            if (!success)
                return new CreateTicketResponse()
                {
                    Error = TicketsCreateErrorType.CreateTicketUnknown,
                    Message = "Error Creating New Ticket",
                };

            return new CreateTicketResponse()
            {
                Error = TicketsCreateErrorType.CreateTicketNoError,
                Message = "Created New Ticket",
            };
        }

        public override async Task<GetTicketResponse> GetTicket(
            GetTicketRequest request,
            ServerCallContext context
        )
        {
            Guid.TryParse(request.TicketId, out var ticketId);
            if (ticketId == Guid.Empty)
                return new GetTicketResponse();

            Guid.TryParse(request.EventId, out var eventId);
            if (eventId == Guid.Empty)
                return new GetTicketResponse();

            var found = await _ticketDataProvider.GetById(ticketId, eventId);
            return new GetTicketResponse() { Record = found };
        }

        public override async Task<GetTicketsForEventResponse> GetTicketsForEvent(
            GetTicketsForEventRequest request,
            ServerCallContext context
        )
        {
            Guid.TryParse(request.EventId, out var eventId);
            if (eventId == Guid.Empty)
                return new GetTicketsForEventResponse();

            var found = await _ticketDataProvider.GetAllByEvent(eventId).ToList();
            var res = new GetTicketsForEventResponse();
            res.Records.AddRange(found);

            return res;
        }
    }
}

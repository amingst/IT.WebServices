using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using IT.WebServices.Fragments.Authorization.Events;
using Microsoft.Extensions.Logging;

namespace IT.WebServices.Authorization.Events.Services
{
    public class EventTicketService : EventTicketInterface.EventTicketInterfaceBase
    {
        private readonly ILogger<EventTicketService> _logger;

        public EventTicketService(ILogger<EventTicketService> logger)
        {
            _logger = logger;
        }

        public override Task<CreateTicketResponse> CreateTicket(
            CreateTicketRequest request,
            ServerCallContext context
        )
        {
            return base.CreateTicket(request, context);
        }

        public override Task<CreateTicketClassResponse> CreateTicketClass(
            CreateTicketClassRequest request,
            ServerCallContext context
        )
        {
            return base.CreateTicketClass(request, context);
        }

        public override Task<GetTicketResponse> GetTicket(
            GetTicketRequest request,
            ServerCallContext context
        )
        {
            return base.GetTicket(request, context);
        }

        public override Task<GetTicketClassResponse> GetTicketClass(
            GetTicketClassRequest request,
            ServerCallContext context
        )
        {
            return base.GetTicketClass(request, context);
        }

        public override Task<GetTicketClassesResponse> GetTicketClasses(
            GetTicketClassesRequest request,
            ServerCallContext context
        )
        {
            return base.GetTicketClasses(request, context);
        }

        public override Task<GetTicketClassesForEventResponse> GetTicketClassesForEvent(
            GetTicketClassesForEventRequest request,
            ServerCallContext context
        )
        {
            return base.GetTicketClassesForEvent(request, context);
        }
    }
}

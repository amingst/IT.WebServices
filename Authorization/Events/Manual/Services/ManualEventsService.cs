using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using IT.WebServices.Fragments.Authorization.Events;
using Microsoft.Extensions.Logging;

namespace IT.WebServices.Authorization.Events.Manual.Services
{
    public class ManualEventsService : EventInterface.EventInterfaceBase
    {
        private readonly ILogger<ManualEventsService> _logger;

        public ManualEventsService(ILogger<ManualEventsService> logger)
        {
            _logger = logger;
        }

        public override Task<CreateEventResponse> CreateEvent(
            CreateEventRequest request,
            ServerCallContext context
        )
        {
            return base.CreateEvent(request, context);
        }

        public override Task<GetEventByIdResponse> GetEventById(
            GetEventByIdRequest request,
            ServerCallContext context
        )
        {
            return base.GetEventById(request, context);
        }

        public override Task<GetEventsResponse> GetEvents(
            GetEventsRequest request,
            ServerCallContext context
        )
        {
            return base.GetEvents(request, context);
        }

        public override Task<ModifyEventResponse> ModifyEvent(
            ModifyEventRequest request,
            ServerCallContext context
        )
        {
            return base.ModifyEvent(request, context);
        }

        public override Task<AnnounceEventResponse> AnnounceEvent(
            AnnounceEventRequest request,
            ServerCallContext context
        )
        {
            return base.AnnounceEvent(request, context);
        }

        public override Task<DeleteEventResponse> DeleteEvent(
            DeleteEventRequest request,
            ServerCallContext context
        )
        {
            return base.DeleteEvent(request, context);
        }

        public override Task<CreateTicketResponse> CreateTicket(
            CreateTicketRequest request,
            ServerCallContext context
        )
        {
            return base.CreateTicket(request, context);
        }

        public override Task<GetTicketResponse> GetTicket(
            GetTicketRequest request,
            ServerCallContext context
        )
        {
            return base.GetTicket(request, context);
        }

        public override Task<GetTicketsResposne> GetTickets(
            GetTicketsRequest request,
            ServerCallContext context
        )
        {
            return base.GetTickets(request, context);
        }

        public override Task<UnAnnounceEventResponse> UnAnnounceEvent(
            UnAnnounceEventRequest request,
            ServerCallContext context
        )
        {
            return base.UnAnnounceEvent(request, context);
        }

        public override Task<UnDeleteEventResponse> UnDeleteEvent(
            UnDeleteEventRequest request,
            ServerCallContext context
        )
        {
            return base.UnDeleteEvent(request, context);
        }

        public override Task<GetTicketsByEventResponse> GetTicketsByEvent(
            GetTicketsByEventRequest request,
            ServerCallContext context
        )
        {
            return base.GetTicketsByEvent(request, context);
        }
    }
}

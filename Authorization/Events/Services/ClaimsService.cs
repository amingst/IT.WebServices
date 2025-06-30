using Grpc.Core;
using IT.WebServices.Authorization.Events.Data;
using IT.WebServices.Fragments.Authorization;
using IT.WebServices.Helpers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IT.WebServices.Authorization.Events.Services
{
    public class ClaimsService : ClaimsInterface.ClaimsInterfaceBase
    {
        private readonly ILogger<ClaimsService> _logger;
        private readonly ITicketDataProvider ticketDataProvider;

        public ClaimsService(ILogger<ClaimsService> logger, ITicketDataProvider ticketDataProvider)
        {
            _logger = logger;
            this.ticketDataProvider = ticketDataProvider;
        }

        public override async Task<GetClaimsResponse> GetClaims(GetClaimsRequest request, ServerCallContext context)
        {
            if (request.UserID == null)
                return new GetClaimsResponse();

            Guid userId;
            if (!Guid.TryParse(request.UserID, out userId))
                return new GetClaimsResponse();

            var res = new GetClaimsResponse();

            var claims = await GetEventClaims(userId);
            res.Claims.AddRange(claims);

            return res;
        }

        private async Task<ClaimRecord[]> GetEventClaims(Guid userId)
        {
            var tickets = await ticketDataProvider.GetAllByUser(userId).ToList();

            var recs = new List<ClaimRecord>();
            recs.AddRange(tickets.Where(t => t.Private.UserId == userId.ToString()).Where(t => t.Public.CreatedOnUTC == null).Where(t => t.Public.UsedOnUTC == null).Select(r => new ClaimRecord()
            {
                Name = r.Public.Title,
                Value = r.TicketId,
                ExpiresOnUTC = r.Public.ExpiredOnUTC,
            }));
            return recs.ToArray();
        }
    }
}

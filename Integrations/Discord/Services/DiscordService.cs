using Grpc.Core;
using IT.WebServices.Fragments.Authorization.Integrations.Discord;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace IT.WebServices.Integrations.Discord.Services
{
    [Authorize]
    public class DiscordService : DiscordInterface.DiscordInterfaceBase
    {
        private readonly ILogger _logger;

        public DiscordService(ILogger<DiscordService> logger)
        {
            _logger = logger;
        }

        public override async Task<EnableDisableDiscordResponse> EnableDiscord(EnableDisableDiscordRequest request, ServerCallContext context)
        {
            throw new NotImplementedException("TODO");
        }

        public override async Task<EnableDisableDiscordResponse> DisableDiscord(EnableDisableDiscordRequest request, ServerCallContext context)
        {
            throw new NotImplementedException("TODO");
        }

        public override async Task<EditDiscordIntegrationResponse> EditDiscordIntegration(EditDiscordIntegrationRequest request, ServerCallContext context)
        {
            throw new NotImplementedException("TODO");
        }
    }
}

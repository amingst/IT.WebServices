using IT.WebServices.Fragments.Authorization.Integrations.Discord;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

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
    }
}

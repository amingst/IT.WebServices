using Grpc.Core;
using Microsoft.Extensions.Logging;
using IT.WebServices.Authentication;
using IT.WebServices.Fragments.Content.Stats;
using System;
using System.Threading.Tasks;
using IT.WebServices.Content.Stats;

namespace IT.WebServices.Content.CMS.Services.Helpers
{
    public class StatsClient
    {
        private readonly IViewService viewService;
        private readonly ILogger logger;

        public StatsClient(IViewService viewService, ILogger<StatsClient> logger)
        {
            this.viewService = viewService;
            this.logger = logger;
        }

        public async Task RecordView(Guid contentId, ONUser user)
        {
            try
            {
                var res = await viewService.LogViewContentInternal(new LogViewContentRequest()
                {
                    ContentID = contentId.ToString(),
                }, user);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error trying to RecordView");
            }
        }
    }
}

using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using IT.WebServices.Authentication;
using IT.WebServices.Content.Stats.Services.Data;
using IT.WebServices.Fragments.Content.Stats;
using System;
using System.Threading.Tasks;

namespace IT.WebServices.Content.Stats.Services
{
    [AllowAnonymous]
    public class ShareService : StatsShareInterface.StatsShareInterfaceBase
    {
        private readonly ILogger logger;
        private readonly IShareDataProvider dataProvider;

        public ShareService(ILogger<ShareService> logger, IShareDataProvider dataProvider)
        {
            this.logger = logger;
            this.dataProvider = dataProvider;
        }

        public override async Task<LogShareContentResponse> LogShareContent(LogShareContentRequest request, ServerCallContext context)
        {
            var userToken = ONUserHelper.ParseUser(context.GetHttpContext());

            if (!Guid.TryParse(request.ContentID, out var contentId))
                return new();

            await dataProvider.LogShare(userToken?.Id ?? Guid.Empty, contentId);

            return new();
        }
    }
}

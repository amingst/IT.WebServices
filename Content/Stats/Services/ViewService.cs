using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using IT.WebServices.Authentication;
using IT.WebServices.Content.Stats.Services.Data;
using IT.WebServices.Fragments.Content.Stats;
using System;
using System.Threading.Tasks;
using IT.WebServices.Fragments.Settings;

namespace IT.WebServices.Content.Stats.Services
{
    [AllowAnonymous]
    public class ViewService : StatsViewInterface.StatsViewInterfaceBase, IViewService
    {
        private readonly ILogger logger;
        private readonly IViewDataProvider dataProvider;

        public ViewService(ILogger<ViewService> logger, IViewDataProvider dataProvider)
        {
            this.logger = logger;
            this.dataProvider = dataProvider;
        }

        public override Task<LogViewContentResponse> LogViewContent(LogViewContentRequest request, ServerCallContext context)
        {
            var userToken = ONUserHelper.ParseUser(context.GetHttpContext());

            return LogViewContentInternal(request, userToken);
        }

        public async Task<LogViewContentResponse> LogViewContentInternal(LogViewContentRequest request, ONUser userToken)
        {
            if (!Guid.TryParse(request.ContentID, out var contentId))
                return new();

            Guid userId = userToken?.Id ?? Guid.Empty;

            await dataProvider.LogView(userId, contentId);

            return new();
        }
    }
}

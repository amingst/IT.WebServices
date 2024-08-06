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
    public class ViewService : StatsViewInterface.StatsViewInterfaceBase
    {
        private readonly ILogger logger;
        private readonly IViewDataProvider dataProvider;

        public ViewService(ILogger<ViewService> logger, IViewDataProvider dataProvider)
        {
            this.logger = logger;
            this.dataProvider = dataProvider;
        }

        public override async Task<LogViewContentResponse> LogViewContent(LogViewContentRequest request, ServerCallContext context)
        {
            var userToken = ONUserHelper.ParseUser(context.GetHttpContext());

            if (!Guid.TryParse(request.ContentID, out var contentId))
                return new();

            await dataProvider.LogView(userToken?.Id ?? Guid.Empty, contentId);

            return new();
        }
    }
}

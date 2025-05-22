using Grpc.Core;
using Microsoft.Extensions.Logging;
using IT.WebServices.Fragments.Generic;
using IT.WebServices.Settings;
using System.Threading.Tasks;
using static IT.WebServices.Fragments.Generic.ServiceStatusResponse.Types;

namespace IT.WebServices.Authorization.Payment.Service
{
    public class ServiceOpsService : ServiceOpsInterface.ServiceOpsInterfaceBase
    {
        private readonly SettingsClient settingsClient;
        private readonly ILogger logger;

        public ServiceOpsService(ILogger<ServiceOpsService> logger, SettingsClient settingsClient)
        {
            this.settingsClient = settingsClient;
            this.logger = logger;
        }

        public override Task<ServiceStatusResponse> ServiceStatus(ServiceStatusRequest request, ServerCallContext context)
        {
            return Task.FromResult(new ServiceStatusResponse() { Status = ServiceStatus(settingsClient) });
        }

        public static OnlineStatus ServiceStatus(SettingsClient settingsClient)
        {
            if (!settingsClient.PublicData.Subscription.Paypal.Enabled)
                return OnlineStatus.Offline;

            if (!settingsClient.PublicData.Subscription.Paypal.IsValid)
                return OnlineStatus.Faulted;

            if (!settingsClient.OwnerData.Subscription.Paypal.IsValid)
                return OnlineStatus.Faulted;

            return OnlineStatus.Online;
        }
    }
}

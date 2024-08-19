using Grpc.Core;
using Grpc.Core.Logging;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using IT.WebServices.Fragments.Authentication;
using System;
using System.Threading.Tasks;

namespace IT.WebServices.Clients
{
    public class ClientGrpcHelper
    {
        private readonly ILogger<ClientGrpcHelper> logger;

        public readonly GrpcChannel ContentServiceChannel;
        public readonly Channel ChatServiceChannel;
        public readonly Channel CommentServiceChannel;
        public readonly Channel NotificationServiceChannel;
        public readonly Channel PaymentServiceChannel;
        public readonly Channel SettingsServiceChannel;
        public readonly Channel StatsServiceChannel;
        public readonly Channel UserServiceChannel;

        private readonly Task<string> ServiceTokenTask;
        public readonly Lazy<string> ServiceToken;

        public ClientGrpcHelper(IConfiguration configuration, ILogger<ClientGrpcHelper> logger)
        {
            this.logger = logger;

            var options = new GrpcChannelOptions
            {
                MaxReceiveMessageSize = null,
                MaxSendMessageSize = null,
            };

            UserServiceChannel = new Channel("localhost", 7001, ChannelCredentials.Insecure);
            ChatServiceChannel = new Channel("localhost", 7001, ChannelCredentials.Insecure);
            CommentServiceChannel = new Channel("localhost", 7001, ChannelCredentials.Insecure);
            SettingsServiceChannel = new Channel("localhost", 7001, ChannelCredentials.Insecure);
            ContentServiceChannel = GrpcChannel.ForAddress(new Uri("http://localhost:7001"), options);
            NotificationServiceChannel = new Channel("localhost", 7001, ChannelCredentials.Insecure);
            PaymentServiceChannel = new Channel("localhost", 7001, ChannelCredentials.Insecure);
            StatsServiceChannel = new Channel("localhost", 7001, ChannelCredentials.Insecure);

            ServiceTokenTask = GetServiceToken();
            ServiceToken = new Lazy<string>(() => ServiceTokenTask.Result);
        }

        private async Task<string> GetServiceToken()
        {
            try
            {
                var client = new ServiceInterface.ServiceInterfaceClient(UserServiceChannel);
                var reply = await client.AuthenticateServiceAsync(new(), null, DateTime.UtcNow.AddSeconds(5));

                return reply?.BearerToken;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in IT.WebServices.Settings.ServiceNameHelper.GetServiceToken");
                return null;
            }
        }
    }
}

using Grpc.Core;
using IT.WebServices.Fragments.Settings;
using System.Threading.Tasks;

namespace IT.WebServices.Clients.Settings
{
    public class SettingsClient
    {
        private ClientGrpcHelper nameHelper;

        public SettingsOwnerData OwnerData { get; private set; }
        public SettingsPrivateData PrivateData { get; private set; }
        public SettingsPublicData PublicData { get; private set; }

        public SettingsClient(ClientGrpcHelper nameHelper)
        {
            this.nameHelper = nameHelper;

            LoadLatestDirect().Wait();
        }

        public async Task LoadLatestDirect()
        {
            var client = new SettingsInterface.SettingsInterfaceClient(nameHelper.SettingsServiceChannel);
            var res = await client.GetOwnerDataAsync(new(), GetMetadata());

            PublicData = res.Public;
        }

        private Metadata GetMetadata()
        {
            var data = new Metadata();
            data.Add("Authorization", "Bearer " + nameHelper.ServiceToken);

            return data;
        }
    }
}

using IT.WebServices.Fragments.Settings;
using System.Threading.Tasks;

namespace IT.WebServices.Settings
{
    public class SettingsClient
    {
        private ServiceNameHelper nameHelper;

        public SettingsPublicData PublicData { get; private set; }

        public SettingsClient(ServiceNameHelper nameHelper)
        {
            this.nameHelper = nameHelper;

            LoadLatestDirect().Wait();
        }

        public async Task LoadLatestDirect()
        {
            var client = new SettingsInterface.SettingsInterfaceClient(nameHelper.SettingsServiceChannel);
            var res = await client.GetPublicDataAsync(new());

            PublicData = res.Public;
        }
    }
}

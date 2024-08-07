using IT.WebServices.Fragments.Settings;
using System.Threading.Tasks;

namespace IT.WebServices.Settings
{
    public class SettingsClient
    {
        private ServiceNameHelper nameHelper;

        public SettingsPublicData PublicData { get; private set; }
        public SettingsPrivateData PrivateData { get; private set; }
        public SettingsOwnerData OwnerData { get; private set; }

        public uint CurrentSettingsId => PublicData.VersionNum;

        public SettingsClient(ServiceNameHelper nameHelper)
        {
            this.nameHelper = nameHelper;
        }

        public async Task LoadLatest()
        {
        }

        public async Task LoadLatestDirect()
        {
            var client = new SettingsInterface.SettingsInterfaceClient(nameHelper.SettingsServiceChannel);
            var res = await client.GetPublicDataAsync(new());

            PublicData = res.Public;
        }

        private void Load(SettingsRecord record)
        {
            PublicData = record.Public;
            PrivateData = record.Private;
            OwnerData = record.Owner;
        }
    }
}

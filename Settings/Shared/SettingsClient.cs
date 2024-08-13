﻿using Grpc.Core;
using IT.WebServices.Fragments.Settings;
using System.Threading.Tasks;

namespace IT.WebServices.Settings
{
    public class SettingsClient
    {
        private ServiceNameHelper nameHelper;

        public SettingsOwnerData OwnerData { get; private set; }
        public SettingsPrivateData PrivateData { get; private set; }
        public SettingsPublicData PublicData { get; private set; }

        public SettingsClient(ServiceNameHelper nameHelper)
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

using IT.WebServices.Fragments.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IT.WebServices.Settings
{
    public class ChannelHelper
    {
        private readonly SettingsClient settingsClient;

        public ChannelHelper(SettingsClient settingsClient)
        {
            this.settingsClient = settingsClient;
        }

        public ChannelRecord[] GetAll()
        {
            return settingsClient.PublicData?.CMS?.Channels?.ToArray();
        }

        public ChannelRecord GetChannelById(string id)
        {
            return settingsClient.PublicData?.CMS?.Channels?.FirstOrDefault(c => c.ChannelId == id);
        }

        public ChannelRecord GetChannelBySlug(string slug)
        {
            return settingsClient.PublicData?.CMS?.Channels?.FirstOrDefault(c => c.UrlStub == slug);
        }
    }
}

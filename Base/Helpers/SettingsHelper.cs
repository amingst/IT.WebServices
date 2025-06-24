using IT.WebServices.Fragments.Settings;
using IT.WebServices.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace IT.WebServices.Helpers
{
    public class SettingsHelper
    {
        private readonly ISettingsService settingsService;
        private readonly Timer timer;

        public SettingsHelper(ISettingsService settingsService)
        {
            this.settingsService = settingsService;

            timer = new Timer();
            timer.Interval = 10000;
            timer.AutoReset = true;
            timer.Enabled = true;
            timer.Elapsed += Timer_Elapsed;

            Load();
        }

        public SettingsOwnerData Owner { get; private set; }
        public SettingsPrivateData Private { get; private set; }
        public SettingsPublicData Public { get; private set; }

        private void Load()
        {
            var res = settingsService.GetOwnerDataInternal().Result;

            Owner = res.Owner;
            Private = res.Private;
            Public = res.Public;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Load();
        }
    }
}

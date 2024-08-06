using IT.WebServices.Fragments.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IT.WebServices.Settings.Services.Data
{
    public interface ISettingsDataProvider
    {
        Task Clear();
        IAsyncEnumerable<SettingsRecord> GetAll();
        Task<SettingsRecord> Get();
        Task Save(SettingsRecord record);
    }
}

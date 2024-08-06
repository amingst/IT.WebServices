using System.Text.Json;

namespace IT.WebServices.Settings.Services.Helpers
{
    public class NeutralNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name) => name;
    }
}

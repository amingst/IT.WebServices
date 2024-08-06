using System.Text.Json;

namespace IT.WebServices.Authentication.Services.Helpers
{
    public class NeutralNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name) => name;
    }
}

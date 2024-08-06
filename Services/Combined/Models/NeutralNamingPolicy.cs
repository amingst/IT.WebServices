using System.Text.Json;

namespace IT.WebServices.Services.Combined.Models
{
    public class NeutralNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name) => name;
    }
}

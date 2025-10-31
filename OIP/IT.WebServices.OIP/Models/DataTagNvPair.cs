using System.Text.Json.Serialization;

namespace IT.WebServices.OIP.Models
{
    public class DataTagNvPair
    {
        public const string CREATOR = "Creator";
        public const string CREATOR_SIGNATURE = "CreatorSig";
        public static readonly DataTagNvPair INDEX_METHOD = new DataTagNvPair() { Name = "Index-Method", Value = "OIP" };
        public static readonly DataTagNvPair VERSION = new DataTagNvPair() { Name = "Ver", Value = "0.9.0" };
        public static readonly DataTagNvPair CONTENT_TYPE = new DataTagNvPair() { Name = "Content-Type", Value = "application/json" };


        [JsonPropertyName("name")]
        [JsonPropertyOrder(1)]
        public string Name { get; set; } = "";

        [JsonPropertyName("value")]
        [JsonPropertyOrder(2)]
        public string Value { get; set; } = "";
    }
}

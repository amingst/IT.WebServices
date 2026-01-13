using System.Text.Json.Serialization;

namespace IT.WebServices.OIP.Models
{
    public class DataForSignature
    {
        [JsonPropertyName("@context")]
        [JsonPropertyOrder(1)]
        public string Context { get; set; } = "did:arweave:not_added_yet";

        [JsonPropertyName("id")]
        [JsonPropertyOrder(2)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Id { get; set; }

        [JsonPropertyName("tags")]
        [JsonPropertyOrder(3)]
        public List<DataTagNvPair> Tags { get; set; } = [DataTagNvPair.INDEX_METHOD, DataTagNvPair.VERSION, DataTagNvPair.CONTENT_TYPE];

        [JsonPropertyName("fragments")]
        [JsonPropertyOrder(4)]
        public List<DidFragments> Fragments { get; set; } = new();
    }
}

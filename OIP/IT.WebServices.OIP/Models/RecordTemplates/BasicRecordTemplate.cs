using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace IT.WebServices.OIP.Models.RecordTemplates
{
    public class BasicRecordTemplate : IRecordTemplate
    {
        [JsonPropertyName("t")]
        public override string Template => "did:arweave:-9DirnjVO1FlbEW1lN8jITBESrTsQKEM_BoZ1ey_0mk";

        [JsonPropertyName("0")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Name { get; set; }

        [JsonPropertyName("1")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Description { get; set; }

        [JsonIgnore]
        public DateTimeOffset? Date { get; set; }
        [JsonPropertyName("2")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public long? DateJson => Date?.ToUnixTimeSeconds();

        [JsonPropertyName("3")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Language { get; set; }

        [JsonPropertyName("4")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Avatar { get; set; }

        [JsonPropertyName("5")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? License { get; set; }

        [JsonPropertyName("6")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? NSFW { get; set; }

        [JsonPropertyName("7")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? CreatorItems { get; set; }

        [JsonPropertyName("8")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? TagItems { get; set; }

        [JsonPropertyName("9")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? NoteItems { get; set; }

        [JsonPropertyName("10")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? UrlItems { get; set; }

        [JsonPropertyName("11")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? Citations { get; set; }

        [JsonPropertyName("12")]
        public string? WebUrl { get; set; }
    }
}

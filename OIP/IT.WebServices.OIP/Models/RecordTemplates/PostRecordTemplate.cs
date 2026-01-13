using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace IT.WebServices.OIP.Models.RecordTemplates
{
    public class PostRecordTemplate : IRecordTemplate
    {
        [JsonPropertyName("t")]
        public override string Template => "did:arweave:op6y-d_6bqivJ2a2oWQnbylD4X_LH6eQyR6rCGqtVZ8";

        [JsonPropertyName("1")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? BylineWriter { get; set; }

        [JsonPropertyName("2")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? BylineWritersTitle { get; set; }

        [JsonPropertyName("3")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? BylineWritersLocation { get; set; }

        [JsonPropertyName("4")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ArticleText { get; set; }

        [JsonPropertyName("5")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? FeaturedImage { get; set; }

        [JsonPropertyName("6")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? ImageItems { get; set; }

        [JsonPropertyName("7")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? ImageCaptionItems { get; set; }

        [JsonPropertyName("8")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? VideoItems { get; set; }

        [JsonPropertyName("9")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? AudioItems { get; set; }

        [JsonPropertyName("10")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? AudioCaptionItems { get; set; }

        [JsonPropertyName("11")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ReplyTo { get; set; }
    }
}

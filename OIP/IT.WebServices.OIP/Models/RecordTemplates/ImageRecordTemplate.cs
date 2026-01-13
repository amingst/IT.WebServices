using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace IT.WebServices.OIP.Models.RecordTemplates
{
    public class ImageRecordTemplate : IRecordTemplate
    {
        [JsonPropertyName("t")]
        public override string Template => "did:arweave:AkZnE1VckJJlRamgNJuIGE7KrYwDcCciWOMrMh68V4o";

        [JsonPropertyName("1")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ArweaveAddress { get; set; }

        [JsonPropertyName("2")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? IPFSAddress { get; set; }

        [JsonPropertyName("3")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? BittorrentAddress { get; set; }

        [JsonPropertyName("4")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Filename { get; set; }

        [JsonPropertyName("5")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ContentType { get; set; }

        [JsonPropertyName("6")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ulong? Size { get; set; }

        [JsonPropertyName("7")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ulong? Width { get; set; }

        [JsonPropertyName("8")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ulong? Length { get; set; }

        [JsonPropertyName("9")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Creator { get; set; }
    }
}

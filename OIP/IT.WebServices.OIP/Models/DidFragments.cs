using IT.WebServices.OIP.Models.RecordTemplates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace IT.WebServices.OIP.Models
{
    public class DidFragments
    {
        [JsonPropertyName("id")]
        [JsonPropertyOrder(1)]
        public string Id { get; set; } = "";

        [JsonPropertyName("dataType")]
        [JsonPropertyOrder(2)]
        public string DataType { get; set; } = "";

        [JsonPropertyName("recordType")]
        [JsonPropertyOrder(3)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? RecordType { get; set; }

        [JsonPropertyName("records")]
        [JsonPropertyOrder(4)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<IRecordTemplate>? Records { get; set; } = new(8);

        //[JsonPropertyName("TemplateName")]
        //[JsonPropertyOrder(3)]
        //[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        //public string? TemplateName { get; set; }

        //[JsonPropertyName("templates")]
        //[JsonPropertyOrder(4)]
        //[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        //public List<ITemplate> Templates { get; set; } = new(8);
    }
}

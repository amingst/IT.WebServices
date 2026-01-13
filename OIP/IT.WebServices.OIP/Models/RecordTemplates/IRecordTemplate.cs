using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace IT.WebServices.OIP.Models.RecordTemplates
{
    [JsonDerivedType(typeof(BasicRecordTemplate))]
    [JsonDerivedType(typeof(CreatorRegistrationRecordTemplate))]
    [JsonDerivedType(typeof(ImageRecordTemplate))]
    [JsonDerivedType(typeof(PostRecordTemplate))]
    public abstract class IRecordTemplate
    {
        [JsonIgnore]
        public abstract string Template { get; }
    }
}

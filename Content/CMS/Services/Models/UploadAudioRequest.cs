using Microsoft.AspNetCore.Http;

namespace IT.WebServices.Content.CMS.Services.Models
{
    public class UploadAudioRequest
    {
        public string Title { get; set; }
        public string Caption { get; set; }
        public string URL { get; set; }
        public string MimeType { get; set; }
        public uint LengthSeconds { get; set; }
        public string OldAssetID { get; set; }
        public IFormFile File { get; set; }
    }
}

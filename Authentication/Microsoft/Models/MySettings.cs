namespace IT.WebServices.Authentication.Services.Microsoft.Models
{
    public class MySettings
    {
        public const string SectionName = "MySettings";

        public string GoodRedirect { get; set; } = "";
        public CookieOrGetEnum CookieOrGet { get; set; } = CookieOrGetEnum.Cookie;

        public enum CookieOrGetEnum
        {
            Cookie,
            Get,
        }
    }
}

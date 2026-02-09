using IT.WebServices.Authentication.Services.Helpers;
using IT.WebServices.Authentication.Services.Microsoft.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Security.Claims;

namespace IT.WebServices.Authentication.Services.Microsoft.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly MySettings settings;
        private readonly TokenHelper tokenHelper;

        public HomeController(IOptions<MySettings> settings, TokenHelper tokenHelper)
        {
            this.settings = settings.Value;
            this.tokenHelper = tokenHelper;
        }

        public async Task<IActionResult> Index()
        {
            var id = GetUserSid();

            if (id == Guid.Empty)
                return Unauthorized();

            var token = await tokenHelper.GenerateToken(id);
            if (string.IsNullOrEmpty(token))
                return Unauthorized();

            Response.Cookies.Append(JwtExtensions.JWT_COOKIE_NAME, token, new CookieOptions()
            {
                HttpOnly = true,
                Expires = DateTimeOffset.UtcNow.AddDays(21),
                IsEssential = true,
                Domain = GetMainDomain(),
            });
            return Redirect(settings.GoodRedirect);
        }

        private string GetMainDomain()
        {
            var host = Request.Host.Host;

            var pieces = host.Split('.');
            if (pieces.Length <= 2)
                return host;

            return string.Join('.', pieces.Skip(pieces.Length - 2));
        }

        public Guid GetUserSid()
        {
            var str = (User?.Identity as ClaimsIdentity)?.Claims?.FirstOrDefault(c => c.Type == "sid")?.Value;
            if (str is null)
                return Guid.Empty;

            if (Guid.TryParse(str, out var guid))
                return guid;

            return Guid.Empty;
        }
    }
}

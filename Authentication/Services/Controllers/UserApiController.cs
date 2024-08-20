using Microsoft.AspNetCore.Mvc;
using System;
using IT.WebServices.Fragments.Generic;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using IT.WebServices.Authentication.Services.Data;
using IT.WebServices.Authentication;

namespace ON.Content.SimpleCMS.Service.Controllers
{
    [AllowAnonymous]
    [Route("/api/auth/user")]
    [ApiController]
    public class UserApiController : Controller
    {
        private readonly ILogger logger;
        private readonly IProfilePicDataProvider picProvider;

        public UserApiController(ILogger<UserApiController> logger, IProfilePicDataProvider picProvider)
        {
            this.logger = logger;
            this.picProvider = picProvider;
        }

        [HttpGet("{userID}/profileimage")]
        public async Task<IActionResult> GetUserProfileImage(string userID)
        {
            if (!Guid.TryParse(userID, out Guid recordId))
                return Redirect("/api/auth/noprofile.png");

            var bytes = await picProvider.GetById(recordId);
            if (bytes == null)
                return Redirect("/api/auth/noprofile.png");

            return File(bytes, "image/png");
        }

        [HttpGet("/api/auth/profileimage")]
        public async Task<IActionResult> GetMyUserProfileImage([FromServices] ONUserHelper userHelper)
        {
            Guid contentId = userHelper.MyUserId;
            if (contentId == Guid.Empty)
                return Redirect("/api/auth/noprofile.png");

            var bytes = await picProvider.GetById(contentId);
            if (bytes == null)
                return Redirect("/api/auth/noprofile.png");

            return File(bytes, "image/png");
        }
    }
}

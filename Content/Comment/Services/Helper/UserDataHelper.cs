using IT.WebServices.Authentication;
using IT.WebServices.Fragments.Authentication;
using IT.WebServices.Fragments.Generic;
using IT.WebServices.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IT.WebServices.Content.Comment.Services.Helper
{
    public class UserDataHelper
    {
        private readonly IUserService userService;

        public UserDataHelper(IUserService userService)
        {
            this.userService = userService;
        }

        public async Task<UserIdRecord> GetRecord(Guid userId)
        {
            var res = await userService.GetOtherPublicUserInternal(userId);

            return new()
            {
                UserID = userId.ToString(),
                DisplayName = res?.Record?.Data?.DisplayName ?? "Unknown",
                UserName = res?.Record?.Data?.UserName ?? "Unknown",
            };
        }
    }
}

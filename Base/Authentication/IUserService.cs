using IT.WebServices.Fragments.Authentication;
using IT.WebServices.Fragments.Settings;
using System;
using System.Threading.Tasks;

namespace IT.WebServices.Authentication
{
    public interface IUserService
    {
        Task<GetUserIdListResponse> GetUserIdListInternal();
        Task<GetOtherPublicUserResponse> GetOtherPublicUserInternal(Guid userId);
        Task<GetOtherPublicUserResponse> GetUserByOldUserID(string oldUserId);
    }
}

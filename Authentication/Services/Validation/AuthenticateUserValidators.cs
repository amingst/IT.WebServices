using System;

namespace IT.WebServices.Fragments.Authentication
{
    internal static class AuthenticateUserValidators
    {
        public static void Validate(AuthenticateUserRequest req, AuthenticateUserResponse res)
        {
            if (string.IsNullOrWhiteSpace(req?.UserName))
                res.AddError("UserName", "UserName is required");

            if (string.IsNullOrWhiteSpace(req?.Password))
                res.AddError("Password", "Password is required");
        }
    }
}

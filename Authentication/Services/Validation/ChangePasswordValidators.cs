using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IT.WebServices.Fragments.Authentication;

namespace IT.WebServices.Authentication.Services.Validation
{
    internal class ChangePasswordValidators
    {
        internal static class ChangeOtherPasswordValidators
        {
            public static void Validate(
                ChangeOtherPasswordRequest req,
                ChangeOtherPasswordResponse res
            )
            {
                if (string.IsNullOrWhiteSpace(req.UserID))
                {
                    res.AddError("UserID", "UserID is required");
                }
                else if (!Guid.TryParse(req.UserID, out _))
                {
                    res.AddError("UserID", "UserID must be a valid GUID");
                }

                if (string.IsNullOrWhiteSpace(req.NewPassword))
                {
                    res.AddError("NewPassword", "NewPassword is required");
                    return;
                }

                ValidatePasswordStrength(req.NewPassword, res);
            }

            private static void ValidatePasswordStrength(
                string password,
                ChangeOtherPasswordResponse res
            )
            {
                if (password.Length < 8)
                    res.AddError("NewPassword", "Password must be at least 8 characters long");

                if (!password.Any(char.IsUpper))
                    res.AddError(
                        "NewPassword",
                        "Password must contain at least one uppercase letter"
                    );

                if (!password.Any(char.IsLower))
                    res.AddError(
                        "NewPassword",
                        "Password must contain at least one lowercase letter"
                    );

                if (!password.Any(char.IsDigit))
                    res.AddError("NewPassword", "Password must contain at least one number");

                if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
                    res.AddError(
                        "NewPassword",
                        "Password must contain at least one special character"
                    );
            }
        }

        internal static class ChangeOwnPasswordValidators
        {
            public static void Validate(ChangeOwnPasswordRequest req, ChangeOwnPasswordResponse res)
            {
                if (string.IsNullOrWhiteSpace(req.OldPassword))
                    res.AddError("OldPassword", "OldPassword is required");

                if (string.IsNullOrWhiteSpace(req.NewPassword))
                {
                    res.AddError("NewPassword", "NewPassword is required");
                    return;
                }

                ValidatePasswordStrength(req.NewPassword, res);
            }

            private static void ValidatePasswordStrength(
                string password,
                ChangeOwnPasswordResponse res
            )
            {
                if (password.Length < 8)
                    res.AddError("NewPassword", "Password must be at least 8 characters long");

                if (!password.Any(char.IsUpper))
                    res.AddError(
                        "NewPassword",
                        "Password must contain at least one uppercase letter"
                    );

                if (!password.Any(char.IsLower))
                    res.AddError(
                        "NewPassword",
                        "Password must contain at least one lowercase letter"
                    );

                if (!password.Any(char.IsDigit))
                    res.AddError("NewPassword", "Password must contain at least one number");

                if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
                    res.AddError(
                        "NewPassword",
                        "Password must contain at least one special character"
                    );
            }
        }
    }
}

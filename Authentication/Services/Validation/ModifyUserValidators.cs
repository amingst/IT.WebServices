using System;
using System.Text.RegularExpressions;

namespace IT.WebServices.Fragments.Authentication
{
    internal static class ModifyOwnUserValidators
    {
        public static void Validate(ModifyOwnUserRequest req, ModifyOwnUserResponse res)
        {
            if (string.IsNullOrWhiteSpace(req.UserID))
            {
                res.AddError("UserID", "UserID is required");
            }
            else if (!Guid.TryParse(req.UserID, out _))
            {
                res.AddError("UserID", "UserID must be a valid GUID");
            }

            if (string.IsNullOrWhiteSpace(req.DisplayName))
            {
                res.AddError("DisplayName", "DisplayName is required");
            }
            else if (req.DisplayName.Length > 100)
            {
                res.AddError("DisplayName", "DisplayName cannot exceed 100 characters");
            }

            if (!string.IsNullOrEmpty(req.Bio) && req.Bio.Length > 500)
                res.AddError("Bio", "Bio cannot exceed 500 characters");

            if (string.IsNullOrWhiteSpace(req.Email))
            {
                res.AddError("Email", "Email is required");
            }
            else
            {
                try
                {
                    var addr = new System.Net.Mail.MailAddress(req.Email.Trim());
                    if (addr.Address != req.Email.Trim())
                        res.AddError("Email", "Invalid email format");
                }
                catch
                {
                    res.AddError("Email", "Invalid email format");
                }
            }
        }
    }
}

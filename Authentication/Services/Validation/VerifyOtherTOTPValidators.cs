using System;
using System.Linq;

namespace IT.WebServices.Fragments.Authentication
{
    internal static class VerifyOtherTOTPValidators
    {
        public static void Validate(VerifyOtherTotpRequest req, VerifyOtherTotpResponse res)
        {
            if (string.IsNullOrWhiteSpace(req?.UserID))
            {
                res.AddError("UserID", "UserID is required");
            }
            else if (!Guid.TryParse(req.UserID, out _))
            {
                res.AddError("UserID", "UserID must be a valid GUID");
            }

            if (string.IsNullOrWhiteSpace(req?.TotpID))
            {
                res.AddError("TotpID", "TotpID is required");
            }
            else if (!Guid.TryParse(req.TotpID, out _))
            {
                res.AddError("TotpID", "TotpID must be a valid GUID");
            }

            if (string.IsNullOrWhiteSpace(req?.Code))
            {
                res.AddError("Code", "Code is required");
            }
            else
            {
                var code = req.Code.Trim();
                if (code.Length != 6 || !code.All(char.IsDigit))
                    res.AddError("Code", "Code must be a 6-digit number");
            }
        }
    }
}

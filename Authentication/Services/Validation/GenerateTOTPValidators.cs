using System;
using System.Linq;

namespace IT.WebServices.Fragments.Authentication
{
    internal static class GenerateOwnTotpValidators
    {
        public static void Validate(GenerateOwnTotpRequest req, GenerateOwnTotpResponse res)
        {
            var deviceName = req?.DeviceName?.Trim();

            if (string.IsNullOrWhiteSpace(deviceName))
            {
                res.AddError("DeviceName", "Device Name is required");
                return;
            }

            if (deviceName.Length > 50)
                res.AddError("DeviceName", "Device Name cannot exceed 50 characters");
        }
    }

    internal static class GenerateOtherTotpValidators
    {
        public static void Validate(GenerateOtherTotpRequest req, GenerateOtherTotpResponse res)
        {
            if (string.IsNullOrWhiteSpace(req?.UserID))
                res.AddError("UserID", "UserID is required");
            else if (!Guid.TryParse(req.UserID, out _))
                res.AddError("UserID", "UserID must be a valid GUID");

            var deviceName = req?.DeviceName?.Trim();
            if (string.IsNullOrWhiteSpace(deviceName))
            {
                res.AddError("DeviceName", "Device Name is required");
                return;
            }

            if (deviceName.Length > 50)
                res.AddError("DeviceName", "Device Name cannot exceed 50 characters");
        }
    }
}

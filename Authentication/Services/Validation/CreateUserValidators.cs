using System.Linq;
using System.Text.RegularExpressions;

namespace IT.WebServices.Fragments.Authentication
{
    internal static class CreateUserValidators
    {
        public static void Validate(CreateUserRequest req, CreateUserResponse res)
        {
            ValidateUserName(req.UserName, res);
            ValidatePassword(req.Password, res);
            ValidateRequired(req.DisplayName, "DisplayName", res);
            ValidateMaxLength(req.DisplayName, 100, "DisplayName", res);
            ValidateEmail(req.Email, res);
            ValidateMaxLength(req.Bio, 500, "Bio", res);
        }

        private static void ValidateRequired(string value, string field, CreateUserResponse res)
        {
            if (string.IsNullOrWhiteSpace(value))
                res.AddError(field, $"{field} is required");
        }

        private static void ValidateMaxLength(
            string value,
            int max,
            string field,
            CreateUserResponse res
        )
        {
            if (!string.IsNullOrEmpty(value) && value.Length > max)
                res.AddError(field, $"{field} cannot exceed {max} characters");
        }

        private static void ValidateUserName(string username, CreateUserResponse res)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                res.AddError("UserName", "Username is required");
                return;
            }

            if (username.Length < 3)
                res.AddError("UserName", "Username must be at least 3 characters");

            if (username.Length > 50)
                res.AddError("UserName", "Username cannot exceed 50 characters");

            if (!Regex.IsMatch(username, @"^[a-zA-Z0-9_]+$"))
                res.AddError(
                    "UserName",
                    "Username can only contain letters, numbers, and underscores"
                );
        }

        private static void ValidatePassword(string password, CreateUserResponse res)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                res.AddError("Password", "Password is required");
                return;
            }

            if (password.Length < 8)
                res.AddError("Password", "Password must be at least 8 characters");
            if (!password.Any(char.IsUpper))
                res.AddError("Password", "Password must contain at least one uppercase letter");
            if (!password.Any(char.IsLower))
                res.AddError("Password", "Password must contain at least one lowercase letter");
            if (!password.Any(char.IsDigit))
                res.AddError("Password", "Password must contain at least one number");
            if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
                res.AddError("Password", "Password must contain at least one special character");
        }

        private static void ValidateEmail(string email, CreateUserResponse res)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                res.AddError("Email", "Email is required");
                return;
            }

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                if (addr.Address != email)
                    res.AddError("Email", "Invalid email format");
            }
            catch
            {
                res.AddError("Email", "Invalid email format");
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IT.WebServices.Fragments.Authentication
{
    public interface IHasValidationErrors
    {
        Google.Protobuf.Collections.RepeatedField<FieldValidationError> ValidationErrors { get; }
    }

    internal static class ValidationHelper
    {
        public static bool HasErrors(IHasValidationErrors response) =>
            response.ValidationErrors != null
            && response.ValidationErrors.Any(fe => fe.Errors != null && fe.Errors.Count > 0);

        public static T AddError<T>(T response, string field, string message)
            where T : class, IHasValidationErrors
        {
            var fe = response.ValidationErrors.FirstOrDefault(e => e.Field == field);
            if (fe == null)
            {
                fe = new FieldValidationError { Field = field };
                response.ValidationErrors.Add(fe);
            }
            fe.Errors.Add(message);
            return response;
        }
    }

    //public partial class CreateUserResponse
    //{
    //    public bool HasErrors() =>
    //        ValidationErrors != null
    //        && ValidationErrors.Any(fe => fe.Errors != null && fe.Errors.Count > 0);

    //    public CreateUserResponse AddError(string field, string message)
    //    {
    //        var fe = ValidationErrors.FirstOrDefault(e => e.Field == field);
    //        if (fe == null)
    //        {
    //            fe = new FieldValidationError { Field = field };
    //            ValidationErrors.Add(fe);
    //        }
    //        fe.Errors.Add(message);
    //        return this;
    //    }
    //}
    public partial class CreateUserResponse : IHasValidationErrors
    {
        public bool HasErrors() => ValidationHelper.HasErrors(this);

        public CreateUserResponse AddError(string field, string message) =>
            ValidationHelper.AddError(this, field, message);
    }

    //public partial class AuthenticateUserResponse
    //{
    //    public bool HasErrors() =>
    //        ValidationErrors != null
    //        && ValidationErrors.Any(fe => fe.Errors != null && fe.Errors.Count > 0);

    //    public AuthenticateUserResponse AddError(string field, string message)
    //    {
    //        var fe = ValidationErrors.FirstOrDefault(e => e.Field == field);
    //        if (fe == null)
    //        {
    //            fe = new FieldValidationError { Field = field };
    //            ValidationErrors.Add(fe);
    //        }
    //        fe.Errors.Add(message);
    //        return this;
    //    }
    //}

    public partial class AuthenticateUserResponse : IHasValidationErrors
    {
        public bool HasErrors() => ValidationHelper.HasErrors(this);

        public AuthenticateUserResponse AddError(string field, string message) =>
            ValidationHelper.AddError(this, field, message);
    }

    public partial class ModifyOtherUserResponse : IHasValidationErrors
    {
        public bool HasErrors() => ValidationHelper.HasErrors(this);

        public ModifyOtherUserResponse AddError(string field, string message) =>
            ValidationHelper.AddError(this, field, message);
    }

    public partial class ModifyOwnUserResponse
    {
        public bool HasErrors() =>
            ValidationErrors != null
            && ValidationErrors.Any(fe => fe.Errors != null && fe.Errors.Count > 0);

        public ModifyOwnUserResponse AddError(string field, string message)
        {
            var fe = ValidationErrors.FirstOrDefault(e => e.Field == field);
            if (fe == null)
            {
                fe = new FieldValidationError { Field = field };
                ValidationErrors.Add(fe);
            }
            fe.Errors.Add(message);
            return this;
        }
    }

    public partial class ChangeOwnPasswordResponse : IHasValidationErrors
    {
        public bool HasErrors() => ValidationHelper.HasErrors(this);

        public ChangeOwnPasswordResponse AddError(string field, string message) =>
            ValidationHelper.AddError(this, field, message);
    }

    public partial class ChangeOtherPasswordResponse : IHasValidationErrors
    {
        public bool HasErrors() => ValidationHelper.HasErrors(this);

        public ChangeOtherPasswordResponse AddError(string field, string message) =>
            ValidationHelper.AddError(this, field, message);
    }

    public partial class GenerateOwnTotpResponse
    {
        public bool HasErrors() =>
            ValidationErrors != null
            && ValidationErrors.Any(fe => fe.Errors != null && fe.Errors.Count > 0);

        public GenerateOwnTotpResponse AddError(string field, string message)
        {
            var fe = ValidationErrors.FirstOrDefault(e => e.Field == field);
            if (fe == null)
            {
                fe = new FieldValidationError { Field = field };
                ValidationErrors.Add(fe);
            }
            fe.Errors.Add(message);
            return this;
        }
    }

    public partial class GenerateOtherTotpResponse
    {
        public bool HasErrors() =>
            ValidationErrors != null
            && ValidationErrors.Any(fe => fe.Errors != null && fe.Errors.Count > 0);

        public GenerateOtherTotpResponse AddError(string field, string message)
        {
            var fe = ValidationErrors.FirstOrDefault(e => e.Field == field);
            if (fe == null)
            {
                fe = new FieldValidationError { Field = field };
                ValidationErrors.Add(fe);
            }
            fe.Errors.Add(message);
            return this;
        }
    }

    public partial class VerifyOwnTotpResponse
    {
        public bool HasErrors() =>
            ValidationErrors != null
            && ValidationErrors.Any(fe => fe.Errors != null && fe.Errors.Count > 0);

        public VerifyOwnTotpResponse AddError(string field, string message)
        {
            var fe = ValidationErrors.FirstOrDefault(e => e.Field == field);
            if (fe == null)
            {
                fe = new FieldValidationError { Field = field };
                ValidationErrors.Add(fe);
            }
            fe.Errors.Add(message);
            return this;
        }
    }

    public partial class VerifyOtherTotpResponse
    {
        public bool HasErrors() =>
            ValidationErrors != null
            && ValidationErrors.Any(fe => fe.Errors != null && fe.Errors.Count > 0);

        public VerifyOtherTotpResponse AddError(string field, string message)
        {
            var fe = ValidationErrors.FirstOrDefault(e => e.Field == field);
            if (fe == null)
            {
                fe = new FieldValidationError { Field = field };
                ValidationErrors.Add(fe);
            }
            fe.Errors.Add(message);
            return this;
        }
    }
}

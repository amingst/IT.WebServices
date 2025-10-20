using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IT.WebServices.Fragments.Authentication;

namespace IT.WebServices.Fragments.Content
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

    public partial class CreateContentResponse : IHasValidationErrors
    {
        public bool HasErrors() => ValidationHelper.HasErrors(this);

        public CreateContentResponse AddError(string field, string message) =>
            ValidationHelper.AddError(this, field, message);
    }

    public partial class ModifyContentResponse : IHasValidationErrors
    {
        public bool HasErrors() => ValidationHelper.HasErrors(this);

        public ModifyContentResponse AddError(string field, string message) =>
            ValidationHelper.AddError(this, field, message);
    }
}

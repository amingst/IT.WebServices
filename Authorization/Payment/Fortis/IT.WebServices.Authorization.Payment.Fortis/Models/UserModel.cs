using IT.WebServices.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IT.WebServices.Authorization.Payment.Fortis.Models
{
    public class UserModel
    {
        public Guid Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        internal static UserModel FromUserToken(ONUser userToken)
        {
            return new()
            {
                Id = userToken.Id,
            };
        }
    }
}

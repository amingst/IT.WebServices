using Antlr4.Runtime.Tree;
using IT.WebServices.Authentication.Services.Data;
using IT.WebServices.Fragments.Authentication;
using IT.WebServices.Fragments.Authorization;
using IT.WebServices.Fragments.Generic;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace IT.WebServices.Authentication.Services.Helpers
{
    public class TokenHelper
    {
        private readonly ClaimsClient claimsClient;
        private readonly IUserDataProvider dataProvider;

        private readonly SigningCredentials creds;

        public TokenHelper(ClaimsClient claimsClient, IUserDataProvider dataProvider)
        {
            this.claimsClient = claimsClient;
            this.dataProvider = dataProvider;

            creds = new SigningCredentials(
                JwtExtensions.GetPrivateKey(),
                SecurityAlgorithms.EcdsaSha256
            );
        }

        public async Task<string> GenerateToken(Guid id)
        {
            var record = await dataProvider.GetById(id);
            if (record == null)
                return string.Empty;

            var otherClaims = await claimsClient.GetOtherClaims(id);

            return GenerateToken(record.Normal, otherClaims);
        }

        public string GenerateToken(UserNormalRecord user, IEnumerable<ClaimRecord> otherClaims)
        {
            var onUser = new ONUser()
            {
                Id = user.Public.UserID.ToGuid(),
                UserName = user.Public.Data.UserName,
                DisplayName = user.Public.Data.DisplayName,
            };

            onUser.Idents.AddRange(user.Public.Data.Identities);
            onUser.Roles.AddRange(user.Private.Roles);

            if (otherClaims != null)
            {
                onUser.ExtraClaims.AddRange(otherClaims.Select(c => new Claim(c.Name, c.Value)));
                onUser.ExtraClaims.AddRange(
                    otherClaims.Select(c => new Claim(
                        c.Name + "Exp",
                        c.ExpiresOnUTC.Seconds.ToString()
                    ))
                );
            }

            return GenerateToken(onUser);
        }

        private string GenerateToken(ONUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var tokenExpiration = DateTime.UtcNow.AddDays(7);
            var claims = user.ToClaims().ToArray();
            var subject = new ClaimsIdentity(claims);
            var token = tokenHandler.CreateJwtSecurityToken(
                null,
                null,
                subject,
                null,
                tokenExpiration,
                DateTime.UtcNow,
                creds
            );

            return tokenHandler.WriteToken(token);
        }
    }
}

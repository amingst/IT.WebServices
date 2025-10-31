using Microsoft.IdentityModel.Tokens;
using NBitcoin;
using NBitcoin.DataEncoders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace IT.WebServices.Crypto.Extra
{
    public static class ExtKeyExtensions
    {
        public static ECDsa ToECDsa(this ExtKey privateKey, ECCurve curve)
        {
            return ECDsa.Create(new ECParameters()
            {
                Curve = curve,
                D = privateKey.PrivateKey.ToBytes()
            });
        }

        public static ECDsa ToECDsa(this Key privateKey, ECCurve curve)
        {
            return ECDsa.Create(new ECParameters()
            {
                Curve = curve,
                D = privateKey.ToBytes()
            });
        }

        public static JsonWebKey ToPrivateJsonWebKey(this Key privateKey)
        {
            var jwk = new JsonWebKey()
            {
                Kty = JsonWebAlgorithmsKeyTypes.EllipticCurve,
                Alg = "ES256K",
                Crv = "secp256k1",
                Use = "sig",
                D = Base64UrlEncoder.Encode(privateKey.ToBytes()),
            };

            return jwk;
        }

        public static string ToPrivateEncodedJsonWebKey(this Key privateKey)
        {
            return Base64UrlEncoder.Encode(CleanEmptyArrs(System.Text.Json.JsonSerializer.Serialize(privateKey.ToPrivateJsonWebKey())));
        }

        private static string CleanEmptyArrs(string str)
        {
            return str.Replace(",\"key_ops\":[]", "").Replace(",\"oth\":[]", "").Replace(",\"x5c\":[]", "");
        }
    }
}

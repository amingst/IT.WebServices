using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IT.WebServices.Crypto
{
    public static class JwkExtension
    {
        public static JsonWebKey DecodeJsonWebKey(this string encodedJWK)
        {
            return new JsonWebKey(Base64UrlEncoder.Decode(encodedJWK));
        }

        public static ECDsa DecodeJsonWebKeyToECDsa(this string encodedJWK)
        {
            var jwk = encodedJWK.DecodeJsonWebKey();
            return jwk.ToECDsa();
        }

        public static ECDsa ToECDsa(this JsonWebKey jwk)
        {
            var ecParams = new ECParameters()
            {
                Curve = GetCurveByName(jwk.Crv),
                Q = new ECPoint(),
            };

            if (!string.IsNullOrWhiteSpace(jwk.D))
                ecParams.D = Base64UrlEncoder.DecodeBytes(jwk.D);

            if (!string.IsNullOrWhiteSpace(jwk.X))
                ecParams.Q.X = Base64UrlEncoder.DecodeBytes(jwk.X);
            if (!string.IsNullOrWhiteSpace(jwk.Y))
                ecParams.Q.Y = Base64UrlEncoder.DecodeBytes(jwk.Y);

            var ecdsa = ECDsa.Create(ecParams);

            return ecdsa;
        }

        public static JsonWebKey ToPrivateJsonWebKey(this ECDsa eckey)
        {
            var parameters = eckey.ExportParameters(true);

            var jwk = new JsonWebKey()
            {
                Kty = JsonWebAlgorithmsKeyTypes.EllipticCurve,
                Use = "sig",
                D = Base64UrlEncoder.Encode(parameters.D),
                Crv = JsonWebKeyECTypes.P256,
                Alg = "ES256"
            };

            return jwk;
        }

        public static string ToPrivateEncodedJsonWebKey(this ECDsa eckey)
        {
            return Base64UrlEncoder.Encode(CleanEmptyArrs(System.Text.Json.JsonSerializer.Serialize(eckey.ToPrivateJsonWebKey())));
        }

        public static JsonWebKey ToPublicJsonWebKey(this ECDsa eckey)
        {
            var parameters = eckey.ExportParameters(false);

            var jwk = new JsonWebKey()
            {
                Kty = JsonWebAlgorithmsKeyTypes.EllipticCurve,
                Use = "sig",
                X = Base64UrlEncoder.Encode(parameters.Q.X),
                Y = Base64UrlEncoder.Encode(parameters.Q.Y),
                Crv = JsonWebKeyECTypes.P256,
                Alg = "ES256"
            };

            return jwk;
        }

        public static string ToPublicEncodedJsonWebKey(this ECDsa eckey)
        {
            return Base64UrlEncoder.Encode(CleanEmptyArrs(System.Text.Json.JsonSerializer.Serialize(eckey.ToPublicJsonWebKey())));
        }

        private static string CleanEmptyArrs(string str)
        {
            return str.Replace(",\"key_ops\":[]", "").Replace(",\"oth\":[]", "").Replace(",\"x5c\":[]", "");
        }

        private static ECCurve GetCurveByName(string curveName)
        {
            switch(curveName)
            {
                case JsonWebKeyECTypes.P256:
                    return ECCurve.NamedCurves.nistP256;
                case JsonWebKeyECTypes.P384:
                    return ECCurve.NamedCurves.nistP384;
                case JsonWebKeyECTypes.P521:
                    return ECCurve.NamedCurves.nistP521;
                case "secp256k1":
                    return CustomCurves.SecP256k1Curve;
                default:
                    throw new NotImplementedException($"Curve not found: {curveName}");
            }
        }
    }
}

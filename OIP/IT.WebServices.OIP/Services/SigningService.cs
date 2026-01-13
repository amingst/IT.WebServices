using IT.WebServices.Crypto;
using IT.WebServices.OIP.Models;
using Microsoft.IdentityModel.Tokens;
using NBitcoin.Crypto;
using NBitcoin.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace IT.WebServices.OIP.Services
{
    public class SigningService
    {
        public static void AddSignatureTag(DataForSignature data, string signingJwk)
        {
            var signature = ComputeSignature(data, signingJwk);

            data.Tags.Add(new DataTagNvPair() { Name = DataTagNvPair.CREATOR_SIGNATURE, Value = signature });
        }

        public static string ComputeSignature(DataForSignature data, string signingJwk)
        {
            var json = JsonSerializer.Serialize(data);
            byte[] messageBytes = Encoding.UTF8.GetBytes(json);

            var signatureByes = ComputeSignature(messageBytes, signingJwk);
            var signature = Base64UrlEncoder.Encode(signatureByes);

            return signature;
        }

        public static byte[] ComputeSignature(byte[] messageBytes, string signingJwk)
        {
            byte[] messageHash = Hashes.SHA256(messageBytes);

            var signatureByes = signingJwk.DecodeJsonWebKeyToECDsa().SignHash(messageHash);

            return signatureByes;
        }
    }
}

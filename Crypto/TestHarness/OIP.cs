using IT.WebServices.Crypto;
using IT.WebServices.Crypto.Extra;
using Microsoft.IdentityModel.Tokens;
using NBitcoin;
using NBitcoin.Crypto;
using NBitcoin.DataEncoders;
using Org.BouncyCastle.Crypto.EC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TestHarness
{
    internal class OIP
    {
        public static void Test()
        {
            var seed = "abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon about";
            var derive = new TheGreatDerivator(seed);
            uint account = 0;
            uint keyNum = 0;
            Console.WriteLine("Using 12 word test seed:");
            Console.WriteLine(seed);

            var signingXPub = derive.DeriveOIPSigningXPub(account);
            Console.WriteLine();
            Console.WriteLine("Generating xPub:");
            Console.WriteLine(signingXPub);
            Console.WriteLine("Should be:");
            Console.WriteLine("xpub6EVyMwjbyrQEwL5Xzt9ffaWdphw5AjUH2Y8dgZsnuvVQ8Bk6t62or1uTudxPYp99Zj9eao1vMopSXQUt7rR18fem1DbT5daw69RrruPJWnv");

            Console.WriteLine();
            Console.WriteLine("Generating Private Key 0:");
            var signingPrivateKey = derive.DeriveOIPSigningPrivateKey(account, keyNum);
            var signingPrivateJwk = signingPrivateKey.ToPrivateEncodedJsonWebKey();
            Console.WriteLine(signingPrivateJwk);
            Console.WriteLine("Should be:");
            Console.WriteLine("eyJhbGciOiJFUzI1NksiLCJjcnYiOiJzZWNwMjU2azEiLCJkIjoieFI3ZXlEenhTVXpPYllnWmxFUjFqbmkybjE0MkhxTFZHRDlhVF9qcXhJMCIsImt0eSI6IkVDIiwidXNlIjoic2lnIn0");

            Console.WriteLine();
            Console.WriteLine("Generating public Key 0:");
            var signingPublicParentKey = signingXPub.FromXPub();
            var signingPublicKey = signingPublicParentKey.Derive(0).PubKey;
            Console.WriteLine(Encoders.Base58Check.EncodeData(signingPublicKey.ToBytes()));
            Console.WriteLine("Should be:");
            Console.WriteLine("87us4UnPJjkmHppa4CrNm6qhALJcTxq7pQKfbBT1S7KMXffXnj");


            string message = "Testing 1... 2... 3...";
            byte[] messageBytes = Encoding.ASCII.GetBytes(message);
            byte[] messageHash = Hashes.SHA256(messageBytes);

            Console.WriteLine();
            Console.WriteLine("Signing message:");
            Console.WriteLine(message);
            Console.WriteLine();
            Console.WriteLine("Computing Hash:");
            Console.WriteLine(Base64UrlEncoder.Encode(messageHash));
            Console.WriteLine("Should be:");
            Console.WriteLine("eG53uNz6azw8LZ5QykHy0HiaMfBBXfzgEvOZmOWLfRo");

            var signature = signingPrivateJwk.DecodeJsonWebKeyToECDsa().SignHash(messageHash);
            Console.WriteLine();
            Console.WriteLine("Computing Signature:");
            Console.WriteLine(Base64UrlEncoder.Encode(signature));

            var ecdsaPub = signingPublicKey.ToECDsa(CustomCurves.SecP256k1Curve);

            var verified = ecdsaPub.VerifyHash(messageHash, signature);
            Console.WriteLine();
            Console.WriteLine("Verifying Signature:");
            Console.WriteLine($"Verified: {verified}");
        }
    }
}

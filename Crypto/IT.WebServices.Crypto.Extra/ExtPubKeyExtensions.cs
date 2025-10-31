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
    public static class ExtPubKeyExtensions
    {
        public static ExtPubKey FromXPub(this string xpubStr)
        {
            return ExtPubKey.Parse(xpubStr, Network.Main); ;
        }

        public static ECDsa ToECDsa(this PubKey pubKey, ECCurve curve)
        {
            var bytes = pubKey.Decompress().ToBytes();
            return ECDsa.Create(new ECParameters()
            {
                Curve = curve,
                Q = new()
                {
                    X = bytes.Skip(1).Take(32).ToArray(),
                    Y = bytes.Skip(33).Take(32).ToArray(),
                }
            });
        }

        public static string ToXPub(this ExtPubKey pubKey)
        {
            return pubKey.ToString(Network.Main);
        }
    }
}

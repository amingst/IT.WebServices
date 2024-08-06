using IT.WebServices.Fragments.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pb = global::Google.Protobuf;

namespace IT.WebServices.Fragments.Authorization
{
    public sealed partial class SubscriptionTier : pb::IMessage<SubscriptionTier>
    {
        public string Label
        {
            get => "$" + (AmountCents / 100.0) + " - " + Name;
        }
    }
}

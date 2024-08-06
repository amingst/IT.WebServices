using System;
using pb = global::Google.Protobuf;

namespace IT.WebServices.Fragments.Authorization.Payment.Paypal
{
    public sealed partial class PaypalPublicSettings : pb::IMessage<PaypalPublicSettings>
    {
        public bool IsValid =>
            (!string.IsNullOrWhiteSpace(Url)) &&
            (!string.IsNullOrWhiteSpace(ClientID));
    }
    public sealed partial class PaypalOwnerSettings : pb::IMessage<PaypalOwnerSettings>
    {
        public bool IsValid => !string.IsNullOrWhiteSpace(ClientSecret);
    }
}

using System;
using pb = global::Google.Protobuf;

namespace IT.WebServices.Fragments.Authorization.Payment.Fortis
{
    public sealed partial class FortisPublicSettings : pb::IMessage<FortisPublicSettings>
    {
        public bool IsValid => true;
    }
    public sealed partial class FortisOwnerSettings : pb::IMessage<FortisOwnerSettings>
    {
        public bool IsValid => !string.IsNullOrWhiteSpace(UserID)
                            && !string.IsNullOrWhiteSpace(UserApiKey)
                            && !string.IsNullOrWhiteSpace(LocationID)
                            && !string.IsNullOrWhiteSpace(ProductID);
    }
}

using System;
using pb = global::Google.Protobuf;

namespace IT.WebServices.Fragments.Authorization.Payment.ParallelEconomy
{
    public sealed partial class ParallelEconomyPublicSettings : pb::IMessage<ParallelEconomyPublicSettings>
    {
        public bool IsValid => true;
    }
    public sealed partial class ParallelEconomyOwnerSettings : pb::IMessage<ParallelEconomyOwnerSettings>
    {
        public bool IsValid => !string.IsNullOrWhiteSpace(UserID)
                            && !string.IsNullOrWhiteSpace(UserApiKey)
                            && !string.IsNullOrWhiteSpace(LocationID)
                            && !string.IsNullOrWhiteSpace(ProductID);
    }
}

using IT.WebServices.Fragments.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pb = global::Google.Protobuf;

namespace IT.WebServices.Fragments.Notification
{
    public sealed partial class UserNotificationSettingsRecord : pb::IMessage<UserNotificationSettingsRecord>
    {
        public Guid UserIDGuid
        {
            get => UserID.ToGuid();
            set => UserID = value.ToString();
        }
    }
}

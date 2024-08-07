using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace IT.WebServices.Content.Stats.Services.Subscriptions
{
    public class SubscriptionList
    {
        public Channel<Guid> ContentChanges = Channel.CreateUnbounded<Guid>();
        public Channel<Guid> UserChanges = Channel.CreateUnbounded<Guid>();
    }
}

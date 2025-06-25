using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IT.WebServices.Fragments.Generic;
using static IT.WebServices.Fragments.Authorization.Events.EventRecurrenceRule;
using pb = global::Google.Protobuf;

namespace IT.WebServices.Fragments.Authorization.Events
{
    public sealed partial class EventRecord : pb::IMessage<EventRecord>
    {
        public Guid EventIDGuid
        {
            get => this.EventId.ToGuid();
            set => this.EventId = value.ToString();
        }

        public bool IsRecurring =>
            this.Public?.StartDate != null
            && this.Public?.EndDate != null
            && this.Public.Recurrence != null
            && this.Public.Recurrence.Frequency != RecurrenceFrequency.RepeatNone;
    }

    public sealed partial class EventTicketRecord : pb::IMessage<EventTicketRecord>
    {
        public Guid TicketIDGuid
        {
            get => this.TicketId.ToGuid();
            set => this.TicketId = value.ToString();
        }

        public Guid EventIDGuid
        {
            get => this.Public.EventId.ToGuid();
            set => this.Public.EventId = value.ToString();
        }
    }
}

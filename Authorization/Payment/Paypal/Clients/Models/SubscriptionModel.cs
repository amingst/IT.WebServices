using IT.WebServices.Fragments.Authorization.Payment;

namespace IT.WebServices.Authorization.Payment.Paypal.Clients.Models
{
    public class SubscriptionModel
    {
        public string? id { get; set; }
        public string? plan_id { get; set; }
        public DateTime start_time { get; set; }
        public BillingInfo? billing_info { get; set; }
        public DateTime create_time { get; set; }
        public DateTime update_time { get; set; }
        public string? status { get; set; }

        public SubscriptionStatus StatusEnum
        {
            get
            {
                switch (status)
                {
                    case "ACTIVE":
                        return SubscriptionStatus.SubscriptionActive;
                    case "CANCELLED":
                        return SubscriptionStatus.SubscriptionStopped;
                    case "SUSPENDED":
                        return SubscriptionStatus.SubscriptionPaused;
                    default:
                        return SubscriptionStatus.SubscriptionUnknown;
                }
            }
        }

        public class BillingInfo
        {
            public LastPayment? last_payment { get; set; }
            public DateTime next_billing_time { get; set; }
            public int failed_payments_count { get; set; }

            public class LastPayment
            {
                public Amount? amount { get; set; }
                public DateTime time { get; set; }

                public class Amount
                {
                    public string? currency_code { get; set; }
                    public string? value { get; set; }
                }
            }
        }
    }
}

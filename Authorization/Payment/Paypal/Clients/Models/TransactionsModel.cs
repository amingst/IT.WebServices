using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IT.WebServices.Authorization.Payment.Paypal.Clients.Models
{
    public class TransactionsModel
    {
        public List<TransactionModel> transactions { get; set; } = new();
    }

    public class TransactionModel
    {
        public string? id { get; set; }
        public string? status { get; set; }
        public AmountWithBreakdown? amount_with_breakdown { get; set; }
        public DateTime time { get; set; }

        public class AmountWithBreakdown
        {
            public GrossAmount? gross_amount { get; set; }

            public class GrossAmount
            {
                public string? value { get; set; }
            }
        }

        public override string ToString()
        {
            return "$" + (amount_with_breakdown?.gross_amount?.value ?? "0.00") + " " + time.ToShortDateString();
        }
    }
}

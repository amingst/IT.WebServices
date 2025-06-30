using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IT.WebServices.Authorization.Payment.Paypal.Clients.Models
{
    public abstract class BasePaginated
    {
        public List<LinksModel>? links { get; set; }

        public class LinksModel
        {
            public string? href { get; set; }
            public string? rel { get; set; }
        }
    }
}

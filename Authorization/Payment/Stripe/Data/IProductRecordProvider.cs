using IT.WebServices.Fragments.Authorization;
using IT.WebServices.Fragments.Authorization.Payment.Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IT.WebServices.Authorization.Payment.Stripe.Data
{
    public interface IProductRecordProvider
    {
        Task<ProductList> GetAll();
        Task SaveAll(ProductList list);
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using IT.WebServices.Fragments.Ecommerce;

namespace IT.WebServices.Ecommerce.Services
{
    public class EcommerceService :  EcommerceInterface.EcommerceInterfaceBase, IEcommerceService
    {
        public EcommerceService() { }
        public override Task<GetCustomersResponse> GetCustomers(GetCustomersRequest request, ServerCallContext context)
        {
            throw new NotImplementedException();
        }
    }
}

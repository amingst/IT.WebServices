using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using IT.WebServices.Fragments.Shops;
using Microsoft.Extensions.Logging;

namespace IT.WebServices.Shops.Combined.Services
{
    public class ShopsService : ShopsInterface.ShopsInterfaceBase
    {
        private readonly ILogger<ShopsService> _logger;

        public ShopsService(ILogger<ShopsService> logger)
        {
            _logger = logger;
        }

        public override async Task<GetProductsResponse> GetProducts(
            GetProductsRequest request,
            ServerCallContext context
        )
        {
            throw new NotImplementedException();
        }

        public override async Task<SyncStoreResponse> SyncStore(
            SyncStoreRequest request,
            ServerCallContext context
        )
        {
            throw new NotImplementedException();
        }
    }
}

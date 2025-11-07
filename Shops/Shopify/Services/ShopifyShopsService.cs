using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using IT.WebServices.Fragments.Shops.Shopify;
using Microsoft.Extensions.Logging;
using ShopifySharp;

namespace IT.WebServices.Shops.Shopify.Services
{
    public class ShopifyShopsService : ShopifyShopsInterface.ShopifyShopsInterfaceBase
    {
        private readonly ILogger<ShopifyShopsService> _logger;
        private readonly IProductService _products;

        public ShopifyShopsService(ILogger<ShopifyShopsService> logger, IProductService products)
        {
            _logger = logger;
            _products = products;
        }

        public override async Task<SyncShopifyStoreResponse> SyncShopifyStore(
            SyncShopifyStoreRequest request,
            ServerCallContext context
        )
        {
            throw new NotImplementedException();
        }
    }
}

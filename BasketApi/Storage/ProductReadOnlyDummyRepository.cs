using System;
using System.Collections.Generic;
using BasketApi.Contracts;
using BasketApi.Domain;
using BasketApi.Contracts.Hal;

namespace BasketApi.Storage
{
    public class ProductReadOnlyStubRepository : IProductReadOnlyRepository
    {
        private readonly Random _random;

        public ProductReadOnlyStubRepository()
        {
            _random = new Random();
        }

        public EmbeddedProduct GetProductPreview(Guid productId)
        {
            return new EmbeddedProduct
            {
                ProductId = productId,
                Description = "Description of product " + productId,
                ProductImageUrl = new Uri($"http://contoso.com/products/assets/{productId}.png"),
                Price = (decimal)_random.NextDouble()*100m,
                _links = new Dictionary<string, HalLink>
                {
                    { "self", new HalLink(new Uri($"http://api.contoso.com/products/{productId}"), "product")}
                }
            };
        }
    }
}
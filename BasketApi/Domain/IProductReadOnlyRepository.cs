using System;
using BasketApi.Contracts;

namespace BasketApi.Domain
{
    public interface IProductReadOnlyRepository
    {
        EmbeddedProduct GetProductPreview(Guid productId);
    }
}

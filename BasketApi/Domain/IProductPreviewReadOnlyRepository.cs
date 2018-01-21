using System;
using BasketApi.Contracts;

namespace BasketApi.Domain
{
    public interface IProductPreviewReadOnlyRepository
    {
        EmbeddedProduct GetProductPreview(Guid productId);
    }
}

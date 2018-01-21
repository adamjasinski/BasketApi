using System;
using System.Collections.Generic;
using System.Text;

namespace BasketApi.Contracts
{

    /// <summary>
    /// Minimal product representation, intended to be embedded inside other related resources
    /// (i.e. product preview, with actual product resource representation being out of scope of this demo).
    /// </summary>
    public class EmbeddedProduct : HalLinkAwareContract
    {
        public Guid ProductId { get; set; }
        public string Description { get; set; }
        public Uri ProductImageUrl { get; set; }
        public decimal Price { get; set; }
    }
}

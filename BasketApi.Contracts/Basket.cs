using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BasketApi.Contracts
{
    /// <summary>
    /// Basket contents representation.
    /// Baskets are identified by user ID of the owner.
    /// </summary>
    public class BasketModel : HalLinkAwareContract
    {
        public Guid Id { get; set; }

        public BasketItemModel[] Items { get; set; } = new BasketItemModel[0];

        public decimal TotalPrice { get; set; }
    }

    /// <summary>
    /// Basket item representation.
    /// Items are identified by ProductId. 
    /// </summary>
    public class BasketItemModel : HalLinkAwareContract
    {
        [Required]
        public Guid ProductId { get; set; }

        [Required]
        [Range(1, Int32.MaxValue)]
        public int Quantity { get; set; }

        //Optional embedded content (preview of related resources)
        public Dictionary<string, object> _embedded;
    }

    /// <summary>
    /// Basket item representation intended to be used for item updates.
    /// </summary>
    public class BasketItemUpdateModel
    {
        [Required]
        [Range(1, Int32.MaxValue)]
        public int Quantity { get; set; }
    }
}

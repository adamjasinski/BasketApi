using System;

namespace BasketApi.Domain
{
    public class BasketItem
    {
        public Guid ProductId { get; }

        public int Quantity { get; private set; }

        public BasketItem(Guid productId, int quantity)
        {
            ProductId = productId;
            Quantity = quantity;
        }

        public void AddQuantity(int quantityDiff)
        {
            Quantity += quantityDiff;
        }

        public void SetQuantity(int newQuantity)
        {
            Quantity = newQuantity;
        }
    }
}
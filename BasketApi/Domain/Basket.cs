using System;
using System.Collections.Generic;

namespace BasketApi.Domain
{
    public class Basket
    {
        public Guid Id { get; }

        private readonly Dictionary<Guid, BasketItem> _items;

        public Basket(Guid id)
        {
            Id = id;
            _items = new Dictionary<Guid, BasketItem>();
        }

        public IReadOnlyCollection<BasketItem> GetItems()
        {
            return _items.Values;
        }

        public BasketItem GetItem(Guid productId)
        {
            return _items.TryGetValue(productId, out var item) ? item : null;
        }

        public void AddItem(Guid productId, int quantity)
        {
            if(_items.TryGetValue(productId, out var existingItem))
            {
                existingItem.AddQuantity(quantity);
                return;
            }

            _items.Add(productId, new BasketItem(productId, quantity));
        }

        public bool DeleteItem(Guid productId)
        {
            if (!_items.ContainsKey(productId))
                return false;
           
            _items.Remove(productId);
            return true;
        }

        public void ClearAllItems()
        {
            _items.Clear();
        }

        public bool UpdateItem(Guid productId, int newQuantity)
        {
            if (!_items.ContainsKey(productId))
                return false;

            var existingItem = _items[productId];
            existingItem.SetQuantity(newQuantity);
            return true;
        }
    }
}

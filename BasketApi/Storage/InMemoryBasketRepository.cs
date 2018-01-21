using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using BasketApi.Domain;
using BasketApi.Modules;

namespace BasketApi.Storage
{
    public class InMemoryBasketRepository : IBasketRepository
    {
        private readonly ConcurrentDictionary<Guid, Basket> _baskets;

        public InMemoryBasketRepository()
        {
            _baskets = new ConcurrentDictionary<Guid, Basket>();
        }

        public void Add(Basket basket)
        {
            if (!_baskets.TryAdd(basket.Id, basket))
            {
                throw new InvalidOperationException("Add error: basket key already exists");
            }
        }

        public Basket Get(Guid id)
        {
            //TODO - clone the value
            return _baskets.TryGetValue(id, out var basket) ? basket : null;
        }

        public void Update(Basket basket)
        {
            var currentBasket = Get(basket.Id);
            if (currentBasket == null)
            {
                throw new InvalidOperationException("Update error: basket key doesn't exist");
            }
            //Note: TryUpdate will compare the ID and ModifiedDate
            if (!_baskets.TryUpdate(basket.Id, basket, currentBasket))
            {
                throw new BasketOptimisticConcurrencyException();
            }
        }
    }

    //class BasketEqualityComparer : IEqualityComparer<Basket>
    //{
    //    public bool Equals(Basket x, Basket y)
    //    {
    //        if (object.ReferenceEquals(x, y))
    //            return true;

    //        if (x == null || y == null)
    //            return false;

    //        return x.Id.Equals(y.Id) && x.ModifiedDate.Equals(y.ModifiedDate);
    //    }

    //    public int GetHashCode(Basket obj)
    //    {
    //        if (obj == null)
    //        {
    //            return 0;
    //        }

    //        return obj.Id.GetHashCode();
    //    }
    //}
}

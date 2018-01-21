using System;
using System.Collections.Concurrent;
using BasketApi.Domain;

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
            return _baskets.TryGetValue(id, out var basket) ? basket : null;
        }

        public void Update(Basket basket)
        {
            var currentBasket = Get(basket.Id);
            if (currentBasket == null)
            {
                throw new InvalidOperationException("Update error: basket key doesn't exist");
            }

            // Note: there's no concurrency control here for updates for the same key 
            // (i.e. if the same user sends several requests at the same time, the last one wins).
            // In order to implement optimistic concurrency (i.e. 'first one wins'), 
            // we'd have to use ETags/LastModifiedDate and If-Matches/If-Unmodified-Since in the API controller,
            // and respond with HTTP 409 (Conflict) if an update conflict was detected.
            _baskets[basket.Id] = basket;
           
        }
    }
}

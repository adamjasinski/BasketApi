using System;
namespace BasketApi.Domain
{
    public interface IBasketRepository
    {
        void Add(Basket basket);

        Basket Get(Guid id);

        void Update(Basket basket);
    }
}

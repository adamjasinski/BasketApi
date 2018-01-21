using System;

namespace BasketApi.Domain
{
    /// <summary>
    /// Service implementing business logic of basket operations.
    /// </summary>
    public class BasketService
    {
        private readonly IBasketRepository _basketRepository;

        public BasketService(IBasketRepository basketRepository)
        {
            _basketRepository = basketRepository;
        }

        public Basket GetBasket(Guid userId)
        {
            var basket = _basketRepository.Get(userId) ?? CreateEmptyBasket(userId);
            return basket;
        }

    
        public BasketItem GetBasketItem(Guid userId, Guid productId)
        {
            var basket = _basketRepository.Get(userId);

            return basket?.GetItem(productId);
        }

        public void AddBasketItem(Guid userId, Guid productId, int quantity)
        {
            var basket = _basketRepository.Get(userId);
            if (basket == null)
            {
                basket = CreateEmptyBasket(userId);
                basket.AddItem(productId, quantity);
                _basketRepository.Add(basket);
            }
            else
            {
                basket.AddItem(productId, quantity);
                _basketRepository.Update(basket);
            }
        }

        public void ClearBasket(Guid userId)
        {
            var basket = _basketRepository.Get(userId);
            if (basket != null)
            {
                basket.ClearAllItems();
                _basketRepository.Update(basket);
            }
        }

        public bool UpdateBasketItemQuantity(Guid userId, Guid productId, int quantity)
        {
            var basket = _basketRepository.Get(userId);
            if (basket == null)
            {
                basket = CreateNewBasketWithSingleItem(userId, productId, quantity);
                _basketRepository.Add(basket);
                return true;
            }

            var updateResult = basket.UpdateItem(productId, quantity);
            if (updateResult)
            {
                _basketRepository.Update(basket);
            }

            return updateResult;
        }

        public bool DeleteBasketItem(Guid userId, Guid productId)
        {
            var basket = _basketRepository.Get(userId);
            if (basket == null)
            {
                return true;
            }

            var updateResult = basket.DeleteItem(productId);
            if (updateResult)
            {
                _basketRepository.Update(basket);
            }

            return updateResult;
        }

        private Basket CreateEmptyBasket(Guid userId)
        {
            return new Basket(userId);
        }

        private Basket CreateNewBasketWithSingleItem(Guid userId, Guid productId, int quantity)
        {
            var basket = CreateEmptyBasket(userId);
            basket.AddItem(productId, quantity);
            return basket;
        }
    }
}

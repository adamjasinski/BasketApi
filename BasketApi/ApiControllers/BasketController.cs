using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using BasketApi.Contracts;
using BasketApi.Contracts.Hal;
using BasketApi.Domain;
using BasketApi.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasketApi.ApiControllers
{
    [Produces(HalMediaTypes.MediaType)]
    [Authorize]
    [ResourceAuthorizationActionFilter]
    [ApiExceptionFilter(IncludeDetails = true)]
    public class BasketController : Controller
    {
        private readonly BasketService _basketService;
        private readonly IProductReadOnlyRepository _productRepository;
        private readonly IMapper _mapper;

        public BasketController(
            BasketService basketService, 
            IProductReadOnlyRepository productRepository, 
            IMapper mapper)
        {
            _basketService = basketService;
            _productRepository = productRepository;
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieves basket content for currently authenticated user.
        /// This route is a convenient alias for a canonical basket URL below.
        /// </summary>
        /// <returns></returns>
        [HttpGet("api/my/basket", Name = "GetBasketForCurrentUser")]
        public IActionResult GetBasketForCurrentUser()
        {
            var userId = ApiClaimHelper.ExtractUserIdClaim(User.Claims);
            if (!userId.HasValue)
                return Unauthorized();
            return GetBasket(userId.Value);
        }

        /// <summary>
        /// Retrieves basket content.
        /// </summary>
        /// <param name="userId">User id</param>
        /// <returns>Basket content</returns>
        /// <remarks>Each autheticated user has a basket, hence the resource always exists.</remarks>
        [HttpGet("api/users/{userId}/basket", Name = "GetBasket")]
        public IActionResult GetBasket(Guid userId)
        {
            var basket = _basketService.GetBasket(userId);

            var basketRepresentation = _mapper.Map<BasketModel>(basket);
            var selfLink = Url.Action("GetBasket", "Basket", new {userId = userId});
            var itemsLink = Url.Action("GetItems", "Basket", new {userId = userId});
            basketRepresentation._links = new Dictionary<string, HalLink>
            {
                {"self", CreateSelfLink(selfLink, "basket")},
                {"items", new HalLink(new Uri(itemsLink, UriKind.RelativeOrAbsolute), "items")}
            };
            EnrichAllBasketItemsWithLinksAndEmbeddedContent(userId, basketRepresentation.Items, false);
            return Ok(basketRepresentation);
        }

       
        /// <summary>
        /// Retrieves basket item for a specific product.
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <param name="productId">Product identifier</param>
        /// <returns></returns>
        [HttpGet("api/users/{userId}/basket/items/{productId}", Name = "GetItem")]
        public IActionResult GetItem(Guid userId, Guid productId)
        {
            var basketItem = _basketService.GetBasketItem(userId, productId);
            if (basketItem == null)
            {
                return NotFound();
            }

            var basketItemRepresentation = _mapper.Map<BasketItemModel>(basketItem);
            EnrichBasketItemWithLinksAndEmbeddedContent(userId, basketItemRepresentation);
            return Ok(basketItemRepresentation);
        }

        /// <summary>
        /// Retrieves basket items.
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <returns></returns>
        [HttpGet("api/users/{userId}/basket/items", Name = "GetItems")]
        public IActionResult GetItems(Guid userId)
        {
            var basket = _basketService.GetBasket(userId);

            var basketItems = basket.GetItems().ToArray();
            var representations = _mapper.Map<BasketItemModel[]>(basketItems);
            EnrichAllBasketItemsWithLinksAndEmbeddedContent(userId, representations, true);

            return Ok(representations);
        }

        /// <summary>
        /// Adds an item to the basket.
        /// If requested product is already in the basket, a relevant quantity of an existing basket item is updated.
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <param name="basketItem">Representation of basket item</param>
        /// <returns></returns>
        [HttpPost("api/users/{userId}/basket/items", Name = "AddItem")]
        public IActionResult AddItem(Guid userId, [FromBody] BasketItemModel basketItem)
        {
            if (!TryValidateModel(basketItem))
            {
                return BadRequest(ModelState);
            }

            _basketService.AddBasketItem(userId, basketItem.ProductId, basketItem.Quantity);

            string createdActionLink = Url.Action("GetItem", "Basket", new {userId = userId, productId = basketItem.ProductId});
            return Created(new Uri(createdActionLink, UriKind.RelativeOrAbsolute), null);
        }

        /// <summary>
        /// Updates basket item.
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <param name="productId">Product identifier</param>
        /// <param name="basketItem">Basket item representation</param>
        /// <remarks>Currently only Quantity updates are supported.</remarks>
        /// <returns></returns>
        [HttpPut("api/users/{userId}/basket/items/{productId}", Name = "UpdateItem")]
        public IActionResult UpdateItem(Guid userId, Guid productId, [FromBody] BasketItemUpdateModel basketItem)
        {
            if (!TryValidateModel(basketItem))
            {
                return BadRequest(ModelState);
            }

            if (!_basketService.UpdateBasketItemQuantity(userId, productId, basketItem.Quantity))
            {
                return NotFound();
            }
            return NoContent();
        }

        /// <summary>
        /// Deletes an item from the basket
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <param name="productId">Product identifier</param>
        /// <returns></returns>
        [HttpDelete("api/users/{userId}/basket/items/{productId}", Name = "DeleteItem")]
        public IActionResult DeleteItem(Guid userId, Guid productId)
        {
            if (!_basketService.DeleteBasketItem(userId, productId))
                return NotFound();
            return NoContent();
        }

        /// <summary>
        /// Deletes all items from the basket
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <returns></returns>
        [HttpDelete("api/users/{userId}/basket/items", Name = "DeleteItems")]
        public IActionResult DeleteItems(Guid userId)
        {
            _basketService.ClearBasket(userId);
            return NoContent();
        }

        private static HalLink CreateSelfLink(string selfAction, string title)
        {
            return new HalLink(new Uri(selfAction, UriKind.RelativeOrAbsolute), title);
        }

        private void EnrichBasketItemWithLinksAndEmbeddedContent(Guid userId, BasketItemModel basketItem, bool includeEmbeddedContent=true)
        {
            var productId = basketItem.ProductId;
            var selfLink = Url.Action("GetItem", "Basket", new { userId = userId, productId = productId });
            basketItem._links =
                new Dictionary<string, HalLink> { { "self", CreateSelfLink(selfLink, "basket") } };
            if (includeEmbeddedContent)
            {
                var productPreview = _productRepository.GetProductPreview(productId);
                basketItem._embedded = new Dictionary<string, object> { { "product", productPreview } };
            }
        }

        private void EnrichAllBasketItemsWithLinksAndEmbeddedContent(Guid userId, BasketItemModel[] basketItems, bool includeEmbeddedContent)
        {
            foreach (var item in basketItems)
            {
                EnrichBasketItemWithLinksAndEmbeddedContent(userId, item, includeEmbeddedContent);
            }
        }
    }

}

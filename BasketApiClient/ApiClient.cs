using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using BasketApi.Contracts;
using BasketApi.Contracts.Hal;
using Newtonsoft.Json;

namespace BasketApiClient
{
    /// <summary>
    /// Client for interacting with Basket Web API
    /// </summary>
    public sealed class ApiClient : IDisposable
    {
        private const string TokenAuthenticationPartialUrl = "/api/token";
        private const string MyBasketPartialUrl = "/api/my/basket";

        private readonly HttpClient _httpClient;

        public bool HasBearerToken { get; private set; }

        public ApiClient(Uri baseUri)
        {
            _httpClient = new HttpClient {BaseAddress = baseUri};
            _httpClient.DefaultRequestHeaders.Accept.Add(HalMediaTypes.HalMediaTypeWithQualityHeader);
        }

        /// <summary>
        /// Authenticates with Basket API.
        /// Retrieves a token from Token endpoint and saves it in the client.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task Authorize(string username, string password)
        {
            var credentialsModel = new CredentialsModel {Username = username, Password = password};
            var uri = new Uri(TokenAuthenticationPartialUrl, UriKind.RelativeOrAbsolute);
            var response = await _httpClient.PostAsync(uri, SerializeContent(credentialsModel));
            ProcessStandardStatusCodes(response);
            var rawToken = await response.Content.ReadAsStringAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", rawToken);
            HasBearerToken = true;
        }

        /// <summary>
        /// Retrieves basket of the currently authenticated user.
        /// </summary>
        /// <returns></returns>
        public async Task<BasketModel> GetOwnBasket()
        {
            var uri = new Uri(MyBasketPartialUrl, UriKind.Relative);
            var response = await _httpClient.GetAsync(uri);
            ProcessStandardStatusCodes(response);
            var responseAsString = await response.Content.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<BasketModel>(responseAsString);
            return model;
        }


        //public async Task<BasketItemModel[]> GetBasketItems(Guid userId)
        //{
        //    //_httpClient.GetAsync(""
        //    throw new NotImplementedException();
        //}

        /// <summary>
        /// Adds an item to a previously retrieved basket.
        /// </summary>
        /// <param name="basket">Previously retrieved basket</param>
        /// <param name="basketItem">Item to add</param>
        /// <returns>URL of the created item</returns>
        public async Task<Uri> AddBasketItem(BasketModel basket, BasketItemModel basketItem)
        {
            var url = ExtractLinkOrThrow(basket, "items");
            return await AddBasketItem(url, basketItem);
        }

        /// <summary>
        /// Adds an item to a basket.
        /// </summary>
        /// <param name="itemsUrl">URL to basket items resource</param>
        /// <param name="basketItem">Item to add</param>
        /// <returns></returns>
        private async Task<Uri> AddBasketItem(Uri itemsUrl, BasketItemModel basketItem)
        {
            var content = SerializeContent(basketItem);
            var response = await _httpClient.PostAsync(itemsUrl, content);
            ProcessStandardStatusCodes(response);
            return response.Headers.Location;
        }

        //public async Task<Uri> AddBasketItem(Guid userId, BasketItemModel basketItem)
        //{
        //    var relativeUri = new Uri($"/users/{userId}/basket/items", UriKind.Relative);
        //    return await AddBasketItem(relativeUri, basketItem);
        //}

        /// <summary>
        /// Retrieves a specific basket item.
        /// </summary>
        /// <param name="basketItemUrl">URL of the basket item</param>
        /// <returns>Basket item</returns>
        public async Task<BasketItemModel> GetBasketItem(Uri basketItemUrl)
        {
            var response = await _httpClient.GetAsync(basketItemUrl);
            ProcessStandardStatusCodes(response);
            var responseAsString = await response.Content.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<BasketItemModel>(responseAsString);
            return model;
        }

        /// <summary>
        /// Updates a basket item identified by a URL.
        /// </summary>
        /// <param name="basketItemUrl">URL of the basket item</param>
        /// <param name="basketItem">Data, with which the basket item should be updated</param>
        /// <returns></returns>
        public async Task UpdateBasketItem(Uri basketItemUrl, BasketItemUpdateModel basketItem)
        {
            var content = SerializeContent(basketItem);
            var response = await _httpClient.PutAsync(basketItemUrl, content);
            ProcessStandardStatusCodes(response);
        }

        ///// <summary>
        ///// Updates a basket item, indicated by .
        ///// </summary>
        ///// <param name="basketItemUrl">URL of the basket item</param>
        ///// <param name="basketItem">Data, with which the basket item should be updated</param>
        ///// <returns></returns>
        //public async Task UpdateBasketItem(BasketItemModel basketItem)
        //{
        //    var url = ExtractSelfLinkOrThrow(basketItem);
        //    var basketItemUpdateModel = new BasketItemUpdateModel { Quantity = basketItem.Quantity };
        //    await UpdateBasketItem(url, basketItemUpdateModel);
        //}

        private static Uri ExtractSelfLinkOrThrow(HalLinkAwareContract contract)
        {
            return ExtractLinkOrThrow(contract, "self");
        }

        private static Uri ExtractLinkOrThrow(HalLinkAwareContract contract, string linkTitle)
        {
            if (!contract._links.TryGetValue(linkTitle, out var halLink) || halLink.Href == null)
                throw new ArgumentException(
                    $"This payload was expected to contain a {linkTitle} link: {contract.GetType().Name}. Please ensure that you retrieved it from the API",
                    nameof(contract));

            return halLink.Href;
        }

        /// <summary>
        /// Deletes a basket item, identified by its URL.
        /// </summary>
        /// <param name="basketItemUrl">URL of the basket item to delete.</param>
        /// <returns></returns>
        public async Task DeleteBasketItem(Uri basketItemUrl)
        {
            var response = await _httpClient.DeleteAsync(basketItemUrl);
            ProcessStandardStatusCodes(response);
        }

        /// <summary>
        /// Deletes a previously retrieved basket item.
        /// </summary>
        /// <param name="basketItem">Previously retrieved basket item</param>
        /// <returns></returns>
        public async Task DeleteBasketItem(BasketItemModel basketItem)
        {
            var url = ExtractSelfLinkOrThrow(basketItem);
            await DeleteBasketItem(url);
        }

        //public async Task DeleteBasketItem(Guid userId, Guid productId)
        //{
        //    var url = new Uri($"/users/{userId}/basket/items/{productId}", UriKind.RelativeOrAbsolute);
        //    await DeleteBasketItem(url);
        //}

        /// <summary>
        /// Clears the basket - deletes all items.
        /// </summary>
        /// <param name="basket">Previously retrieved basket</param>
        /// <returns>Cleared basket (containing no items)</returns>
        public async Task<BasketModel> ClearBasket(BasketModel basket)
        {
            var url = ExtractLinkOrThrow(basket, "items");
            await ClearBasket(url);
            return await GetOwnBasket();
        }

        /// <summary>
        /// Clears the basket - deletes all items.
        /// </summary>
        /// <param name="itemsUrl">URL to basket items</param>
        /// <returns>Cleared basket (containing no items)</returns>
        private async Task ClearBasket(Uri itemsUrl)
        {
            await _httpClient.DeleteAsync(itemsUrl);
        }

        //public async Task ClearBasket(Guid userId)
        //{
        //    var url = new Uri($"/users/{userId}/basket/items", UriKind.RelativeOrAbsolute);
        //    await ClearBasket(url);
        //}

        private void ProcessStandardStatusCodes(HttpResponseMessage responseMessage)
        {
            switch (responseMessage.StatusCode)
            {
                case HttpStatusCode.Accepted:
                case HttpStatusCode.Created:
                case HttpStatusCode.OK:
                    return;
                case HttpStatusCode.NotFound:
                    throw new ResourceNotFoundException(responseMessage.RequestMessage.RequestUri);
                default:
                    responseMessage.EnsureSuccessStatusCode();
                    return;
            }
        }

        private static StringContent SerializeContent(object basketItem)
        {
            return new StringContent(JsonConvert.SerializeObject(basketItem), Encoding.UTF8, "application/json");
        }


        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }

  
}

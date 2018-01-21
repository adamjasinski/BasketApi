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

        /// <summary>
        /// Creates and instance of the API client
        /// </summary>
        /// <param name="baseUri">The base URL of the API (i.e. the scheme://host:port segment)</param>
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


        /// <summary>
        /// Adds an item to a previously retrieved basket.
        /// </summary>
        /// <param name="basket">Previously retrieved basket</param>
        /// <param name="basketItem">Item to add</param>
        /// <returns>URL of the created item</returns>
        public async Task<Uri> AddBasketItem(BasketModel basket, BasketItemModel basketItem)
        {
            var url = basket.ExtractLink("items");
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
            var url = basketItem.ExtractSelfLink();
            await DeleteBasketItem(url);
        }

        /// <summary>
        /// Clears the basket - deletes all items.
        /// </summary>
        /// <param name="basket">Previously retrieved basket</param>
        /// <returns>Refreshed basket (containing no items)</returns>
        public async Task ClearBasket(BasketModel basket)
        {
            var url = basket.ExtractLink("items");
            await ClearBasket(url);
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

        private void ProcessStandardStatusCodes(HttpResponseMessage responseMessage)
        {
            switch (responseMessage.StatusCode)
            {
                case HttpStatusCode.Accepted:
                case HttpStatusCode.Created:
                case HttpStatusCode.NoContent:
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


        /// <summary>
        /// Disposes of the API client.
        /// </summary>
        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }

  
}

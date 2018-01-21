using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using BasketApi.Contracts;
using BasketApiClient;
using NUnit.Framework;

namespace BasketApi.IntegrationTests
{
    [TestFixture]
    public class BasketScenarios
    {
        private ApiClient _client;
        private Fixture _fixture;

        [SetUp]
        public void CreateApiClient()
        {
            _client = new ApiClient(new Uri(Constants.ApiBaseUrl));
            _fixture = new Fixture();
        }

        [Test]
        public async Task AuthenticatedUserCanAlwaysRetrieveBasket()
        {
            await AuthenticateWithNewUser();

            var basket = await _client.GetOwnBasket();
            Assert.AreEqual(0, basket.Items.Length);
            Assert.NotNull(basket._links);
            Assert.NotNull(basket._links["self"]);
        }

        [Test]
        public async Task AddingNewItemToBasket_CanRetrieve()
        {
            await AuthenticateWithNewUser();

            var basket = await _client.GetOwnBasket();
            _fixture.Customize(new BasketItemForCreationCustomization());
            var basketItemToAdd = _fixture.Create<BasketItemModel>();

            var basketItemUrl = await _client.AddBasketItem(basket, basketItemToAdd);
            basket = await _client.GetOwnBasket();

            Assert.IsNotNull(basketItemUrl);
            Assert.AreEqual(1, basket.Items.Length);
            Assert.AreEqual(basketItemToAdd.ProductId, basket.Items[0].ProductId);
            Assert.AreEqual(basketItemToAdd.Quantity, basket.Items[0].Quantity);
        }

        [Test]
        public async Task AddingMultipleItemsToBasket_CanRetrieveBasket()
        {
            await AuthenticateWithNewUser();

            var basket = await _client.GetOwnBasket();
            _fixture.Customize(new BasketItemForCreationCustomization());
            var basketItemsToAdd = _fixture.CreateMany<BasketItemModel>(3).ToArray();

            await _client.AddBasketItem(basket, basketItemsToAdd[0]);
            await _client.AddBasketItem(basket, basketItemsToAdd[1]);
            await _client.AddBasketItem(basket, basketItemsToAdd[2]);
            basket = await _client.GetOwnBasket();

            Assert.AreEqual(3, basket.Items.Length);
            Assert.AreEqual(basketItemsToAdd[0].ProductId, basket.Items[0].ProductId);
            Assert.AreEqual(basketItemsToAdd[1].ProductId, basket.Items[1].ProductId);
            Assert.AreEqual(basketItemsToAdd[2].ProductId, basket.Items[2].ProductId);
        }

        [Test]
        public async Task AddingMultipleItemsToBasket_CanRetrieveIndividualItems()
        {
            await AuthenticateWithNewUser();

            var basket = await _client.GetOwnBasket();
            _fixture.Customize(new BasketItemForCreationCustomization());
            var basketItemsToAdd = _fixture.CreateMany<BasketItemModel>(2).ToArray();

            var uri1 = await _client.AddBasketItem(basket, basketItemsToAdd[0]);
            var uri2 = await _client.AddBasketItem(basket, basketItemsToAdd[1]);
            var retrievedItem1 = await _client.GetBasketItem(uri1);
            var retrievedItem2 = await _client.GetBasketItem(uri2);

            Assert.AreEqual(basketItemsToAdd[0].ProductId, retrievedItem1.ProductId);
            Assert.AreEqual(basketItemsToAdd[0].Quantity, retrievedItem1.Quantity);
            Assert.AreEqual(basketItemsToAdd[1].ProductId, retrievedItem2.ProductId);
            Assert.AreEqual(basketItemsToAdd[1].Quantity, retrievedItem2.Quantity);
        }

        [Test]
        public async Task AddingMultipleItemsToBasket_CanDeleteIndividualItems()
        {
            await AuthenticateWithNewUser();

            var basket = await _client.GetOwnBasket();
            _fixture.Customize(new BasketItemForCreationCustomization());
            var basketItemsToAdd = _fixture.CreateMany<BasketItemModel>(2).ToArray();

            var uri1 = await _client.AddBasketItem(basket, basketItemsToAdd[0]);
            var uri2 = await _client.AddBasketItem(basket, basketItemsToAdd[1]);

            await _client.DeleteBasketItem(uri1);
            await _client.DeleteBasketItem(uri2);

            basket = await _client.GetOwnBasket();
            Assert.AreEqual(0, basket.Items.Length);
        }

        [Test]
        public async Task AddingMultipleItemsToBasket_CanRetrieveAndThenDeleteIndividualItems()
        {
            await AuthenticateWithNewUser();

            var basket = await _client.GetOwnBasket();
            _fixture.Customize(new BasketItemForCreationCustomization());
            var basketItemsToAdd = _fixture.CreateMany<BasketItemModel>(2).ToArray();

            var uri1 = await _client.AddBasketItem(basket, basketItemsToAdd[0]);
            var uri2 = await _client.AddBasketItem(basket, basketItemsToAdd[1]);
            var retrievedItem1 = await _client.GetBasketItem(uri1);
            var retrievedItem2 = await _client.GetBasketItem(uri2);

            await _client.DeleteBasketItem(retrievedItem1);
            await _client.DeleteBasketItem(retrievedItem2);

            basket = await _client.GetOwnBasket();
            Assert.AreEqual(0, basket.Items.Length);
        }

        [Test]
        public async Task AddingMultipleItemsToBasket_ClearBasket_ShouldDeleteAllItems()
        {
            await AuthenticateWithNewUser();

            var basket = await _client.GetOwnBasket();
            _fixture.Customize(new BasketItemForCreationCustomization());
            var basketItemsToAdd = _fixture.CreateMany<BasketItemModel>(2).ToArray();

            await _client.AddBasketItem(basket, basketItemsToAdd[0]);
            await _client.AddBasketItem(basket, basketItemsToAdd[1]);

            await _client.ClearBasket(basket);
            //And re-read the basket
            basket = await _client.GetOwnBasket();
            Assert.AreEqual(0, basket.Items.Length);
        }

        [Test]
        public async Task AddingMultipleItemsWithSameProductIdToBasket_CanRetrieve_AndQuantitiesShouldBeSummed()
        {
            await AuthenticateWithNewUser();

            var basket = await _client.GetOwnBasket();
            _fixture.Customize(new BasketItemForCreationCustomization());
            _fixture.Freeze<Guid>(); //will re-use the same Guid for productId
            var basketItemsToAdd = _fixture.CreateMany<BasketItemModel>(3).ToArray();

            await _client.AddBasketItem(basket, basketItemsToAdd[0]);
            await _client.AddBasketItem(basket, basketItemsToAdd[1]);
            await _client.AddBasketItem(basket, basketItemsToAdd[2]);
            basket = await _client.GetOwnBasket();

            Assert.AreEqual(1, basket.Items.Length);
            var expectedQuantity = basketItemsToAdd.Sum(x => x.Quantity);
            Assert.AreEqual(expectedQuantity, basket.Items[0].Quantity);
        }

        [Test]
        public async Task AddingNewItemToBasket_AndUpdatingQuantity_CanRetrieveWithUpdatedQuantity()
        {
            await AuthenticateWithNewUser();

            var basket = await _client.GetOwnBasket();
            _fixture.Customize(new BasketItemForCreationCustomization());
            var basketItemToAdd = _fixture.Create<BasketItemModel>();

            var basketItemUrl = await _client.AddBasketItem(basket, basketItemToAdd);
            var basketItemToUpdate = _fixture.Create<BasketItemUpdateModel>();

            await _client.UpdateBasketItem(basketItemUrl, basketItemToUpdate);
            var retrievedBasketItem = await _client.GetBasketItem(basketItemUrl);

            Assert.AreEqual(basketItemToUpdate.Quantity, retrievedBasketItem.Quantity);
        }

        [Test]
        public async Task AddingNewItemToBasket_AndUpdatingWithInvalidQuantity_ShouldThrow()
        {
            await AuthenticateWithNewUser();

            var basket = await _client.GetOwnBasket();
            _fixture.Customize(new BasketItemForCreationCustomization());
            var basketItemToAdd = _fixture.Create<BasketItemModel>();

            var basketItemUrl = await _client.AddBasketItem(basket, basketItemToAdd);
            var basketItemToUpdate = new BasketItemUpdateModel {Quantity = 0};

            var exc = Assert.ThrowsAsync<HttpRequestException>(() =>
                _client.UpdateBasketItem(basketItemUrl, basketItemToUpdate));
            Assert.That(exc.Message.Contains("400"));
        }

        private async Task AuthenticateWithNewUser()
        {
            var newUserName = _fixture.Create<string>();
            await _client.Authorize(newUserName, Constants.UniversalPassword);
            //pre-condition check
            Assert.IsTrue(_client.HasBearerToken);
        }

        [TearDown]
        public void TearDown()
        {
            _client.Dispose();
        }
    }

}

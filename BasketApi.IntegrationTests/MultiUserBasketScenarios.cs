using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using BasketApi.Contracts;
using BasketApiClient;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace BasketApi.IntegrationTests
{
    [TestFixture]
    public class MultiUserBasketScenarios
    {
        private ApiClient _client1;
        private ApiClient _client2;
        private Fixture _fixture;

        [SetUp]
        public void CreateApiClients()
        {
            _client1 = new ApiClient(new Uri(Constants.ApiBaseUrl));
            _client2 = new ApiClient(new Uri(Constants.ApiBaseUrl));
            _fixture = new Fixture();
        }

        [Test]
        public async Task AddingNewItemToBasketOfOneUser_DoesNotAffectOtherUser()
        {
            await AuthenticateWithTwoNewUsers();

            var basket1 = await _client1.GetOwnBasket();
            _fixture.Customize(new BasketItemForCreationCustomization());
            var basketItemToAdd1 = _fixture.Create<BasketItemModel>();

            await _client1.AddBasketItem(basket1, basketItemToAdd1);
            basket1 = await _client1.GetOwnBasket();
            var basket2 = await _client2.GetOwnBasket();

            Assert.AreEqual(1, basket1.Items.Length);
            Assert.AreEqual(0, basket2.Items.Length);
        }


        [Test]
        public async Task AddingNewItemsToBasketsOfDifferentUsers_CanRetrieve()
        {
            await AuthenticateWithTwoNewUsers();

            var basket1 = await _client1.GetOwnBasket();
            var basket2 = await _client2.GetOwnBasket();
            _fixture.Customize(new BasketItemForCreationCustomization());
            var basketItemToAdd1 = _fixture.Create<BasketItemModel>();
            var basketItemToAdd2 = _fixture.Create<BasketItemModel>();

            var basketItemUrl1 = await _client1.AddBasketItem(basket1, basketItemToAdd1);
            var basketItemUrl2 = await _client2.AddBasketItem(basket2, basketItemToAdd2);

            basket1 = await _client1.GetOwnBasket();
            basket2 = await _client2.GetOwnBasket();

            Assert.IsNotNull(basketItemUrl1);
            Assert.IsNotNull(basketItemUrl2);
            Assert.AreEqual(1, basket1.Items.Length);
            Assert.AreEqual(1, basket2.Items.Length);
            Assert.AreEqual(basketItemToAdd1.ProductId, basket1.Items[0].ProductId);
            Assert.AreEqual(basketItemToAdd2.ProductId, basket2.Items[0].ProductId);
        }

        [Test]
        public async Task AddingNewItemsToBasketsOfDifferentUsers_AttemptToRetrieveSomeoneElsesBasketShouldThrowIndicatingForbidden()
        {
            await AuthenticateWithTwoNewUsers();

            var basket1 = await _client1.GetOwnBasket();
            var basket2 = await _client2.GetOwnBasket();
            _fixture.Customize(new BasketItemForCreationCustomization());
            var basketItemToAdd1 = _fixture.Create<BasketItemModel>();
            var basketItemToAdd2 = _fixture.Create<BasketItemModel>();

            var basketItemUrl1 = await _client1.AddBasketItem(basket1, basketItemToAdd1);
            var basketItemUrl2 = await _client2.AddBasketItem(basket2, basketItemToAdd2);


            //User1 trying to retrieve URL belonging to User2 - should result in HTTP 403
            var exc = Assert.ThrowsAsync<HttpRequestException>(() =>
                _client1.GetBasketItem(basketItemUrl2));
            Assert.That(exc.Message.Contains("403"));
        }

        private async Task AuthenticateWithTwoNewUsers()
        {
            var newUserName1 = "user" + _fixture.Create<string>();
            var newUserName2 = "user" + _fixture.Create<string>();
            await _client1.Authorize(newUserName1, Constants.UniversalPassword);
            await _client2.Authorize(newUserName2, Constants.UniversalPassword);
        }

        [TearDown]
        public void TearDown()
        {
            _client1.Dispose();
            _client2.Dispose();
        }
    }
}

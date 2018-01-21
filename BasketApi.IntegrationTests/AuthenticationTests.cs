using System;
using System.Net.Http;
using System.Threading.Tasks;
using BasketApiClient;
using NUnit.Framework;

namespace BasketApi.IntegrationTests
{
    [TestFixture]
    public class AuthenticationTests
    {
        private ApiClient _client;

        [SetUp]
        public void CreateApiClient()
        {
            _client = new ApiClient(new Uri("http://localhost:56128"));
        }

        [Test]
        public async Task ValidCredentials_ShouldRetrieveToken()
        {
            await _client.Authorize("adam", Constants.UniversalPassword);
            Assert.IsTrue(_client.HasBearerToken);
        }

        [Test]
        public void InvalidCredentials_ShouldThrow()
        {
            Assert.ThrowsAsync<UnauthorizedException>(() => 
                _client.Authorize("otheruser", "unknown password"));
        }

        [Test]
        public void UnauthenticatedUserAccessingBasketApi_ShouldThrow()
        {
            Assert.ThrowsAsync<UnauthorizedException>(() => 
                _client.GetOwnBasket());
        }

        [TearDown]
        public void TearDown()
        {
            _client.Dispose();
        }
    }
}

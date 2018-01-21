using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Claims;

namespace BasketApi.Infrastructure
{
    public interface IAuthenticationService
    {
        bool Authenticate(string username, string password, out List<Claim> claims);
    }
    public class AuthenticationServiceStub : IAuthenticationService
    {
        public const string Issuer = "BasketApi.Security.Bearer";
        public const string Audience = "BasketApi.Security.Bearer";
        public const string SecretValue = "secret value eulav terces";
        private const string UniversalPassword = "demo";
    
        /// <summary>
        /// Cache of username-userId pairs
        /// </summary>
        private readonly ConcurrentDictionary<string, Guid> _userCache = new ConcurrentDictionary<string, Guid>();

        public bool Authenticate(string username, string password, out List<Claim> claims)
        {
            claims = null;
            if (password != UniversalPassword)
                return false;
            var userId = _userCache.GetOrAdd(username, x => Guid.NewGuid());

            claims = new List<Claim>
            {
                new Claim(ApiClaimTypes.UserId, userId.ToString()),
                new Claim(ClaimTypes.Name, username)
            };
            return true;
        }
    }
}

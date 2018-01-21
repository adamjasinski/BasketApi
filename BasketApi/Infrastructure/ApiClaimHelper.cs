using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace BasketApi.Infrastructure
{
    public static class ApiClaimHelper
    {
        public static Guid? ExtractUserIdClaim(IEnumerable<Claim> claims)
        {
            var userIdClaim = claims.SingleOrDefault(x => x.Type == ApiClaimTypes.UserId);
            if (string.IsNullOrEmpty(userIdClaim?.Value))
                return null;
            return Guid.Parse(userIdClaim.Value);
        }

        //public static Guid? ExtractUserIdClaim(IEnumerable<Claim> claims)
        //{
        //    UserId claim extraction always happens after authentication
        //    var userIdClaim = claims.SingleOrDefault(x => x.Type == ApiClaimTypes.UserId);
        //    if (string.IsNullOrEmpty(userIdClaim?.Value))
        //        return null;
        //    return Guid.Parse(userIdClaim.Value);
        //}
    }

    public class ApiClaimTypes
    {
        public const string UserId = "UserId";
    }
}

using System;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.WebUtilities;

namespace BasketApi.Infrastructure
{
    public class ResourceAuthorizationActionFilterAttribute : ActionFilterAttribute
    {
        readonly Regex userPathSegment = new Regex("/users/([\\w-]+)");

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            if (context.HttpContext.User == null)
                return;

            var userSegmentMatch = userPathSegment.Match(context.HttpContext.Request.Path);
            if (!userSegmentMatch.Success)
                return;

            var requestedResourceUserId = userSegmentMatch.Groups.Last().Value;

            //var queryStrings = QueryHelpers.ParseQuery(context.HttpContext.Request.QueryString.Value);
            //if (!queryStrings.TryGetValue("userId", out var requestedResourceUserId))
            //    return;

            var authenticatedUserId = ApiClaimHelper.ExtractUserIdClaim(context.HttpContext.User.Claims);
            if(authenticatedUserId == null)
                throw new UnauthorizedAccessException();

            if (requestedResourceUserId != authenticatedUserId.Value.ToString())
                throw new ResourceAccessNotPermittedException();
        }
    }
}

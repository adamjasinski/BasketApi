using System;
using System.Collections.Generic;
using BasketApi.Contracts.Hal;
using Microsoft.AspNetCore.Mvc;

namespace BasketApi.ApiControllers
{
    public class RootsController : Controller
    {
        [HttpGet("/api")]
        public IActionResult Get()
        {
            var roots = new Dictionary<string, HalLink>
            {
                {"basket", new HalLink(new Uri(this.Url.Action("GetBasketForCurrentUser", "Basket"), UriKind.RelativeOrAbsolute), "my-basket") },
                {"token", new HalLink(new Uri(this.Url.Action("Create", "Token"), UriKind.RelativeOrAbsolute), "token") },

            };
            return Ok(roots);
        }
    }
}

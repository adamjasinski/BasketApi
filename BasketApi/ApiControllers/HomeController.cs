using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace BasketApi.ApiControllers
{
    [Route("/")]
    public class HomeController
    {
        public IActionResult Get()
        {
            return new ContentResult
            {
                Content = @"<p>Basket API Demo</p><p><a href=""/api"">Root resources index</a></p>"
                          + "<p>Please use a REST API testing tool like Postman or Fiddler</p>",
                ContentType = "text/html"
            };
        }
    }
}

using System.Collections.Generic;
using BasketApi.Contracts.Hal;

namespace BasketApi.Contracts
{
    public abstract class HalLinkAwareContract
    {
        //HAL links
        public Dictionary<string, HalLink> _links;
    }
}
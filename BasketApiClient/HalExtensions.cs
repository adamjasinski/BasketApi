using System;
using BasketApi.Contracts;

namespace BasketApiClient
{
    public static class HalExtensions
    {
        public static Uri ExtractSelfLink(this HalLinkAwareContract contract)
        {
            if (contract == null)
                throw new ArgumentNullException(nameof(contract));
            return contract.ExtractLink("self");
        }

        public static Uri ExtractLink(this HalLinkAwareContract contract, string linkTitle)
        {
            if (contract == null)
                throw new ArgumentNullException(nameof(contract));

            if (!contract._links.TryGetValue(linkTitle, out var halLink) || halLink.Href == null)
                throw new ArgumentException(
                    $"This payload was expected to contain a {linkTitle} link: {contract.GetType().Name}. Please ensure that you retrieved it from the API",
                    nameof(contract));

            return halLink.Href;
        }
    }
}

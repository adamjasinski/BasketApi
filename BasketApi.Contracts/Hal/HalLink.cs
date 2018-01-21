using System;

namespace BasketApi.Contracts.Hal
{
    /// <summary>
    /// Represents a HAL link
    /// </summary>
    public class HalLink
    {
        public Uri Href { get; set; }
        public string Title { get; set; }

        public HalLink()
        {
            
        }

        public HalLink(Uri href)
        {
            Href = href;
        }

        public HalLink(Uri href,  string title)
            : this(href)
        {
            Title = title;
        }
    }
}

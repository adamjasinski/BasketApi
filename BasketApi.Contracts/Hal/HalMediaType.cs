using System.Net.Http.Headers;

namespace BasketApi.Contracts.Hal
{
    public static class HalMediaTypes
    {
        public const string MediaType = "application/hal+json";
        public static MediaTypeHeaderValue HalMediaTypeHeader { get; } = new MediaTypeHeaderValue(MediaType);
        public static MediaTypeWithQualityHeaderValue HalMediaTypeWithQualityHeader { get; } = new MediaTypeWithQualityHeaderValue(MediaType);
    }
}

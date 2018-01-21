using System;

namespace BasketApiClient
{
    public class ResourceNotFoundException : Exception
    {
        public ResourceNotFoundException(Uri resourceUri)
            : base("The desired resource hasn't been found: " + resourceUri)
        {
        }
    }
}

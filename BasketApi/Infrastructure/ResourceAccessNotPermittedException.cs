using System;

namespace BasketApi.Infrastructure
{
    /// <summary>
    /// An exception indicating that an authenticated user isn't allowed to access resource.
    /// (should be translated to HTTP 403 by a custom exception filter)
    /// </summary>
    public class ResourceAccessNotPermittedException : Exception
    {

    }
}
using System;

namespace BasketApiClient
{
    /// <summary>
    /// An exception indicating that the user didn't provide valid authentication credentials.
    /// (corresponds to HTTP 401)
    /// </summary>
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException() 
            : base("The request has not been applied because it lacks valid authentication credentials for the target resource.")
        {
        }
    }

    /// <summary>
    /// An exception indicating that an authenticated user isn't allowed to access resource.
    /// (corresponds to HTTP 403)
    /// </summary>
    public class ResourceAccessNotPermittedException : Exception
    {
        public ResourceAccessNotPermittedException()
            : base("User provided valid credentials, but isn't allowed to access this resource.")
        {

        }
    }

    /// <summary>
    /// An exception indicating that The server cannot or will not process the request due to something that is perceived to be a client error.
    /// (corresponds to HTTP 400)
    /// </summary>
    public class BadRequestException : Exception
    {
        public BadRequestException(string errorDetails)
            : base("Request data is invalid. Details: " + errorDetails)
        {

        }
    }

    public class ResourceNotFoundException : Exception
    {
        public ResourceNotFoundException(Uri resourceUri)
            : base("The desired resource hasn't been found: " + resourceUri)
        {
        }
    }
}

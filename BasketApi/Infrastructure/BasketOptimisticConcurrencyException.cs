using System;

namespace BasketApi.Modules
{
    public class BasketOptimisticConcurrencyException : Exception
    {
        public BasketOptimisticConcurrencyException()
            : base("Update error: optimistic concurrency exception. The user should reload the basket and repeat the operation")
        {
        }
    }
}
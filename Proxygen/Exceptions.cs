using System;
using System.Collections.Generic;

namespace Proxygen
{
    public class ProxygenExceptions
    {
        public class CardsNotFoundException : Exception
        {
            public ICollection<string> NotFoundCards = null!;
            
            private CardsNotFoundException()
            {
            }

            private CardsNotFoundException(string message)
            : base(message)
            {
            }
            
            private CardsNotFoundException(string message, Exception innerException)
            : base(message, innerException)
            {
            }

            public CardsNotFoundException(ICollection<string> notFoundCards)
            {
                NotFoundCards = notFoundCards;
            }
        }
        
    }
}
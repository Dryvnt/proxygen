using System;
using System.Collections;
using System.Collections.Generic;
using ScryfallApi.Client.Apis;

namespace Proxygen
{
    public class ProxygenExceptions
    {
        public class CardsNotFoundException : Exception
        {
            public ICollection<string> NotFoundCards;
            
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
using System;

namespace Injector.PropertyLoader.Exceptions
{
    public class NotResourceException : Exception
    {
        public NotResourceException(Type type) : base($"{type} is not resource.")
        {
            
        }
    }
}
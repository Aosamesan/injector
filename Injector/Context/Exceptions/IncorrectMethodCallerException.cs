using System;

namespace Injector.Context.Exceptions
{
    public class IncorrectMethodCallerException : Exception
    {
        public IncorrectMethodCallerException() : base("Incorrect Method Caller")
        {
            
        }
        
    }
}
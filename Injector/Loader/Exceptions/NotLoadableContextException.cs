using System;

namespace Injector.Loader.Exceptions
{
    public class NotLoadableContextException : Exception
    {
        public NotLoadableContextException(Type type) : base($"{type} is not loadable context.") { }
    }
}
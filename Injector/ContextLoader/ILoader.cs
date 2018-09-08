using System;
using Injector.Context;
using Injector.Context.Attributes;

namespace Injector.ContextLoader
{
    public interface ILoader
    {
        [ContextConfiguableMethod]
        IContext LoadContext(Type contextType);
    }
}
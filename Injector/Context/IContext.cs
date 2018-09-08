using System;

namespace Injector.Context
{
    public interface IContext
    {
        T Get<T>(string name = null);
        object Get(string name);
        object Get(Type type);
        bool Contains(string name);
        bool Contains(Type type);
    }

    internal interface IContextBuilder<out TBuilder> : IContext where TBuilder : IContextBuilder<TBuilder>
    {
        TBuilder Add(string name, object obj);
        TBuilder Add(object obj);
    }
}
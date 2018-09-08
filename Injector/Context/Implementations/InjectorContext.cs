using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using Injector.Context.Attributes;
using Injector.Context.Exceptions;
using Injector.Helpers;

namespace Injector.Context.Implementations
{
    public class InjectorContext : IContext
    {
        protected IDictionary<string, object> nameDictionary;
        protected IDictionary<Type, object> typeDictionary;

        private InjectorContext() {}
        
        private InjectorContext(IDictionary<string, object> nameDictionary, IDictionary<Type, object> typeDictionary)
        {
            this.nameDictionary = nameDictionary;
            this.typeDictionary = typeDictionary;
        }

        public class InjectorContextBuilder : InjectorContext, IContextBuilder<InjectorContextBuilder>
        {
            private readonly ISet<Type> anonymousObjectTypeSet;

            public InjectorContextBuilder()
            {
                nameDictionary = new Dictionary<string, object>();
                typeDictionary = new Dictionary<Type, object>();
                anonymousObjectTypeSet = new HashSet<Type>();
            }
            
            public InjectorContextBuilder Add(string name, object obj)
            {
                AttributeHelper.CheckCorrectCall<ContextConfiguableMethod>();
                var type = obj.GetType();
                nameDictionary[name] = obj;
                if (!anonymousObjectTypeSet.Contains(type))
                {
                    typeDictionary[type] = obj;
                }

                return this;
            }

            public InjectorContextBuilder Add(object obj)
            {
                AttributeHelper.CheckCorrectCall<ContextConfiguableMethod>();
                var type = obj.GetType();
                if (anonymousObjectTypeSet.Contains(type))
                {
                    throw new DuplicatedObjectException(type);
                }

                typeDictionary[type] = obj;
                anonymousObjectTypeSet.Add(type);

                return this;
            }

            public IContext Build()
            {
                return new InjectorContext(nameDictionary, typeDictionary);
            }
        }


        public T Get<T>(string name = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                return (T)typeDictionary[typeof(T)];
            }
            return (T)nameDictionary[name];
        }

        public object Get(string name)
        {
            return nameDictionary[name];
        }

        public object Get(Type type)
        {
            return typeDictionary[type];
        }

        public bool Contains(string name)
        {
            return nameDictionary.ContainsKey(name);
        }

        public bool Contains(Type type)
        {
            return typeDictionary.ContainsKey(type);
        }
    }
}
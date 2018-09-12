using System;
using System.Linq;
using Injector.Helpers;

namespace Injector.PropertyLoader.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class PropertySource : Attribute
    {
        public Type[] SourceTypes { get; }

        public PropertySource(params Type[] types)
        {
            SourceTypes = types;
        }

        public PropertySource() : this(Array.Empty<Type>())
        {
            
        }

        public bool CheckResources()
        {
            return (from Type type in SourceTypes select ReflectionHelper.IsResourceTypeClass(type)).Aggregate(true,
                CommonHelper.Conjunction);
        }
    }
}
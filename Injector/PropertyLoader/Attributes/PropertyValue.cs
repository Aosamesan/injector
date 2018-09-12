using System;

namespace Injector.PropertyLoader.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyValue : Attribute
    {
        public string Name { get; }

        public PropertyValue(string name)
        {
            Name = name;
        }
    }
}
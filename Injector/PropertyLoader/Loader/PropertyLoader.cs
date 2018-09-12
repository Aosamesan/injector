using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using Injector.Helpers;
using Injector.PropertyLoader.Attributes;
using Injector.PropertyLoader.Exceptions;

namespace Injector.PropertyLoader.Loader
{
    public class PropertyLoader
    {
        public IDictionary<string, string> Load(Type configurationType)
        {
            IDictionary<string, string> resultDictionary = new Dictionary<string, string>();
            
            // Check whether type has [PropertySource] attribute or not
            if (!AttributeHelper.IsTypeMarked<PropertySource>(configurationType))
            {
                return resultDictionary;
            }

            var propertySource = AttributeHelper.getAttribute<PropertySource>(configurationType);

            if (propertySource == null)
            {
                return resultDictionary;
            }
            
            foreach (var sourceType in propertySource.SourceTypes)
            {
                foreach (var pair in ReadProperty(sourceType))
                {
                    resultDictionary[pair.Item1] = pair.Item2;
                }
            }

            return resultDictionary;
        }

        private IEnumerable<(string, string)> ReadProperty(Type resourceType)
        {
            return from PropertyInfo propertyInfo
                    in resourceType.GetProperties()
                where propertyInfo.PropertyType == typeof(string)
                select (propertyInfo.Name,  ReflectionHelper.getStaticProperty<ResourceManager>(resourceType).GetString(propertyInfo.Name));
        }
    }
}
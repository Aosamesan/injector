using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using Injector.Loader.Exceptions;

namespace Injector.Helpers
{
    public static class ReflectionHelper
    {
        public static bool IsInstantiableClass(Type type)
        {
            if (type.IsClass)
            {
                return !(type.IsAbstract && type.IsSealed);
            }

            return false;
        }

        public static void CheckInstantiableClass(Type type)
        {
            var instantiable = IsInstantiableClass(type);
            var parameterlessCntr = HasParameterlessPublicConstructor(type); 
            if (!instantiable || !parameterlessCntr)
            {
                Console.WriteLine("{0}, {1}", instantiable, parameterlessCntr);
                throw new NotLoadableContextException(type);
            }
        }

        public static bool HasParameterlessPublicConstructor(Type type)
        {
            if (type.IsClass)
            {
                return (from ConstructorInfo constructorInfo
                            in type.GetConstructors()
                        select constructorInfo.GetParameters()?.Length == 0
                    ).Aggregate(false, CommonHelper.Disjunction);
            }
            Console.WriteLine("{0} is not class type", type);

            return false;
        }
        

        public static bool HasParameterPublicConstructor(Type type)
        {
            if (type.IsClass)
            {
                return (from ConstructorInfo constructorInfo
                            in type.GetConstructors(BindingFlags.Public)
                        select constructorInfo.GetParameters()?.Length != 0
                    ).Aggregate(false, CommonHelper.Disjunction);
            }

            return false;
        }

        public static bool IsResourceTypeClass(Type type)
        {
            if (type.IsClass)
            {
                return (from PropertyInfo property
                            in type.GetProperties()
                        select property.PropertyType == typeof(ResourceManager) &&
                               property.Name == nameof(ResourceManager))
                    .Aggregate(false, CommonHelper.Disjunction);
            }

            return false;
        }

        public static TProperty getStaticProperty<TProperty>(Type type) where TProperty : class
        {
            var property = (from PropertyInfo propertyInfo
                    in type.GetProperties()
                where propertyInfo.PropertyType == typeof(TProperty)
                select propertyInfo.GetValue(null)).FirstOrDefault() as TProperty;
            return property;
        }
    }
}
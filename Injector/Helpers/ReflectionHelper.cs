using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Injector.ContextLoader.Exceptions;

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
            if (!IsInstantiableClass(type))
            {
                throw new NotLoadableContextException(type);
            }
        }

        public static bool HasParameterlessPublicConstructor(Type type)
        {
            if (type.IsClass)
            {
                return (from ConstructorInfo constructorInfo
                            in type.GetConstructors(BindingFlags.Public)
                        select constructorInfo.GetParameters()?.Length == 0
                    ).Aggregate(false, CommonHelper.Disjunction);
            }

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
        
    }
}
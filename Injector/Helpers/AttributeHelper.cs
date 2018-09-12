using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Injector.Context.Exceptions;
using Injector.Loader.Exceptions;

namespace Injector.Helpers
{
    public static class AttributeHelper
    {
        public static TAttribute getAttribute<TAttribute>(Type type) where TAttribute : Attribute
        {
            return (from Attribute attr in type.GetCustomAttributes() where attr is TAttribute select attr)
                .FirstOrDefault() as TAttribute;
        }
        
        public static TAttribute getAttribute<TAttribute>(PropertyInfo propertyInfo) where TAttribute : Attribute
        {
            return (from Attribute attr in propertyInfo.GetCustomAttributes() where attr is TAttribute select attr)
                .FirstOrDefault() as TAttribute;
        }
        
        public static bool IsTypeMarked<TClass, TAttribute>() where TAttribute : Attribute
        {
            return IsTypeMarked<TAttribute>(typeof(TClass));
        }
        
        public static bool IsTypeMarked<TAttribute>(Type type) where TAttribute : Attribute
        {
            return (from Attribute attr
                    in type.GetCustomAttributes()
                select attr is TAttribute).Aggregate(false, CommonHelper.Disjunction);
        }

        public static bool IsMethodMarked<TAttribute>(MethodBase methodBase) where TAttribute : Attribute
        {
            return (from Attribute attr
                    in methodBase.GetCustomAttributes<Attribute>()
                    select attr is TAttribute).Aggregate(false, CommonHelper.Disjunction);
        }

        public static bool IsParameterMarked<TAttribute>(ParameterInfo parameterInfo) where TAttribute : Attribute
        {
            return (from Attribute attr
                    in parameterInfo.GetCustomAttributes<Attribute>()
                select attr is TAttribute).Aggregate(false, CommonHelper.Disjunction);
        }

        public static bool IsPropertyMarked<TAttribute>(PropertyInfo propertyInfo) where TAttribute : Attribute
        {
            return (from Attribute attr
                    in propertyInfo.GetCustomAttributes<Attribute>()
                select attr is TAttribute && propertyInfo.CanWrite).Aggregate(false, CommonHelper.Disjunction);
        }
        
        public static void CheckCorrectCall<TAttribute>() where TAttribute : Attribute
        {
            var callerMethodBase = new StackTrace().GetFrame(2).GetMethod();
            if (!IsMethodMarked<TAttribute>(callerMethodBase))
            {
                throw new IncorrectMethodCallerException();
            }
        }

        public static void CheckHasTypeAttribute<TAttribute, TException>(Type type) where TAttribute : Attribute where TException : Exception
        {
            if (IsTypeMarked<TAttribute>(type)) return;
            if (!(Activator.CreateInstance(typeof(TException), type) is Exception e))
            {
                throw new Exception($"{type} does not have {typeof(TAttribute).Name} and {typeof(TException).Name} has no constructor.");
            }

            throw e;
        }
        
        public static IEnumerable<string> GetNamespaceToScan(Type contextType)
        {
            if ((from Attribute attr in contextType.GetCustomAttributes<Attribute>()
                where attr is Context.Attributes.Context
                select attr).First() is Context.Attributes.Context contextAttribute)
            {
                return contextAttribute.NamespaceToScan;
            }
            return ImmutableList<string>.Empty;
        }

        public static IEnumerable<TSource> GetMethodBases<TSource>(this IEnumerable<TSource> sources, Predicate<TSource> predicate) where TSource : MethodBase
        => from TSource source in sources where predicate(source) select source;

        public static IEnumerable<TSource> GetMethodBaseWithParameter<TSource, TAttribute>(this IEnumerable<TSource> sources) where TSource : MethodBase where TAttribute : Attribute
        => GetMethodBases(sources, source => IsMethodMarked<TAttribute>(source) && source.GetParameters()?.Length != 0);

        public static IEnumerable<TSource> GetMethodBaseWithoutParameter<TSource, TAttribute>(this IEnumerable<TSource> sources) where TSource : MethodBase where TAttribute : Attribute
        => GetMethodBases(sources, source => IsMethodMarked<TAttribute>(source) && source.GetParameters()?.Length == 0);

    }
}
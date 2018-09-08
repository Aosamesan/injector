using System;
using System.Collections.Generic;
using System.Linq;

namespace Injector.Helpers
{
    public static class SelectHelper
    {
        public static IEnumerable<TSource> Find<TSource>(this IEnumerable<TSource> source, Predicate<TSource> predicate)
            => from TSource item in source where predicate(item) select item;

        public static TSource FindOne<TSource>(this IEnumerable<TSource> source, Predicate<TSource> predicate)
            => (from TSource item in source where predicate(item) select item).FirstOrDefault();
        
        

    }
}
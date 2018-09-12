using System;

namespace Injector.Helpers
{
    public class CommonHelper
    {
        public static readonly Func<bool, bool, bool> Disjunction = (a, b) => a || b;
        public static readonly Func<bool, bool, bool> Conjunction = (a, b) => a && b;
    }
}
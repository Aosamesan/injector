using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using Injector.Helpers;

namespace DependencyInjection
{
    class Program {
        public static void Main(string[] args)
        {
            var fooValue = GetValue(typeof(Foo));
            var barValue = GetValue(typeof(Bar));

            Console.WriteLine(fooValue.Aggregate((a, b) => a + ", " + b));
            Console.WriteLine(barValue.Aggregate((a, b) => a + ", " + b));

        }

        public static string[] GetValue(Type type)
        {
            var attr = type.GetCustomAttribute<TestAttribute>();
            return attr?.Values;
        }

        public static string[] GetStrings(params string[] args)
        {
            return args;
        }

        [TestAttribute(Values = new []{"foo", "bar"})]

        public class Foo
        {
            
        }

        [TestAttribute(Values = new []{"baz"})]
        public class Bar
        {
            
        }
        
    }

    [AttributeUsage(AttributeTargets.Class)]
    class TestAttribute : Attribute
    {
        public string[] Values { get; set; }
    }
}
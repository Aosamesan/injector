using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using Injector.Context;
using Injector.Context.Attributes;
using Injector.ContextLoader;
using Injector.Helpers;

namespace DependencyInjection
{
    class Program {
        public static void Main(string[] args)
        {
            IContext context = ContextLoader.LoadContext(typeof(TestConfig));
            string testString = context.Get<string>("testString");
            Console.WriteLine(testString);
        }
        
    }

    [Context]
    public class TestConfig
    {
        [Instantiate]
        public string TestString() {
            return "foo";
        }
    }
}
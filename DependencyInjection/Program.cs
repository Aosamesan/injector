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
            IContext context = GenericContextLoader.CreateContextLoader<TestConfig>().LoadContext();
            string testString = context.Get<string>("testString");
            Console.WriteLine(testString);
            string testString2 = context.Get<string>("testString2");
            Console.WriteLine(testString2);
            TestItem testItem = context.Get<TestItem>("testItem");
            testItem.Print();
        }
        
    }

    [Context("DependencyInjection")]
    public class TestConfig
    {
        [Instantiate]
        public string TestString() {
            return "foo";
        }

        [Instantiate]
        public string FooBar()
        {
            return "baz";
        }

        [Instantiate]
        public string TestString2([Autowired] string testString)
        {
            return testString + "bar";
        }
    }

    [Instantiate]
    public class TestItem
    {
        [Autowired("fooBar")]
        public string FooBar { get; set; }
        private string testString;
        private string testString2;

        public TestItem([Autowired] string testString, [Autowired] string testString2)
        {
            this.testString = testString;
            this.testString2 = testString2;
        }

        public void Print()
        {
            Console.WriteLine("FooBar : {0}, testString : {1}, testString2 : {2}", FooBar, testString, testString2);
        }

    }
}
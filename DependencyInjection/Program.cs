using System;
using System.Linq;
using System.Reflection;
using System.Resources;
using Injector.Context;
using Injector.Context.Attributes;
using Injector.Helpers;
using Injector.Loader;
using Injector.PropertyLoader.Attributes;

namespace DependencyInjection
{
    class Program {
        public static void Main(string[] args)
        {
            Test();
        }
        
        public static void Test() {
            var context = ContextLoader.CreateContextLoader<TestConfig>().LoadContext();
            var testString = context.Get<string>("testString");
            Console.WriteLine(testString);
            var testString2 = context.Get<string>("testString2");
            Console.WriteLine(testString2);
            var testItem = context.Get<TestItem>("testItem");
            testItem.Print();
            Console.WriteLine(testItem.Foo);
            Console.WriteLine(context.Get<string>("testString3"));
        }
        
    }

    [Context("DependencyInjection")]
    [PropertySource(typeof(TestResources), typeof(TestResources2))]
    public class TestConfig
    {
        [PropertyValue("foo")]
        public string Foo { get; set; }
        
        [PropertyValue("bar")]
        public string Bar { get; set; }
        
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

        [Instantiate]
        public string TestString3([Autowired("testString2")] string s)
        {
            return Foo + s + Bar;
        }
    }

    [Scannable]
    public class TestItem
    {
        [PropertyValue("foo")]
        public string Foo { get; set; }
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
            Console.WriteLine("FooBar : {0}, testString : {1}, testString2 : {2}, Foo : {0}", FooBar, testString, testString2, Foo);
        }

    }
}
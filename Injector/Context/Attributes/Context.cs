using System;

namespace Injector.Context.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class Context : Attribute
    {
        public string[] NamespaceToScan { get; set; }
    }
}
using System;

namespace Injector.Context.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class Context : Attribute
    {
        string[] namespaceToScan = Array.Empty<string>();
        public string[] NamespaceToScan { get => namespaceToScan; set => namespaceToScan = value; }
    }
}
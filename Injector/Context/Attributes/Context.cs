using System;

namespace Injector.Context.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class Context : Attribute
    {
        string[] namespaceToScan;
        public string[] NamespaceToScan { get => namespaceToScan; set => namespaceToScan = value; }
        public Context()
        {
            namespaceToScan = Array.Empty<string>();
        }
        public Context(params string[] namespaces)
        {
            namespaceToScan = namespaces;
        }
    }
}
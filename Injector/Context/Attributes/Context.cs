using System;

namespace Injector.Context.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class Context : Attribute
    {
        public string[] NamespaceToScan { get; }
        
        public Context()
        {
            NamespaceToScan = Array.Empty<string>();
        }
        
        public Context(params string[] namespaces)
        {
            NamespaceToScan = namespaces;
        }
    }
}
using System;

namespace Injector.Context.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class Instantiate : Attribute
    {
        public string Name { get; set; }
    }
}
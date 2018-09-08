using System;

namespace Injector.Context.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter)]
    public class Autowired : Attribute
    {
        public string Name { get; set; }
    }
}
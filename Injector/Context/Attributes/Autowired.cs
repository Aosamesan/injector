using System;

namespace Injector.Context.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
    public class Autowired : Attribute
    {
        public string Name { get; set; }
        public Autowired(string name)
        {
            Name = name;
        }
        public Autowired()
        {
            Name = null;
        }
    }
}
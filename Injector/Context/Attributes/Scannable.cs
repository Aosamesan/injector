using System;

namespace Injector.Context.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class Scannable : Instantiate
    {
    }
}
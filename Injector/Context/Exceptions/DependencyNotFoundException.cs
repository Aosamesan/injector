using System;
using Injector.Loader.Models;

namespace Injector.Context.Exceptions
{
    public class DependencyNotFoundException : Exception
    {
        public DependencyNotFoundException(InstantiateModel model) : base($"Can not instantiate : {model.Name}")
        {
        }
        
    }
}
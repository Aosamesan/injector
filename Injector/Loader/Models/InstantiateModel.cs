using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Injector.Helpers;

namespace Injector.Loader.Models
{
    public sealed class InstantiateModel
    {
        public bool IsConstructorType { get; }
        public string Name { get; }
        public Type Type { get; }
        public int Weight { get; private set; }
        public MethodBase Method { get; }

        public InstantiateModel(Type type, string name)
        {
            Name = NamingHelper.ConvertToCamelCase(name);
            Type = type;
            Method = type.GetConstructors()[0];
            Weight = Method.GetParameters().Length;
            IsConstructorType = true;
        }

        public InstantiateModel(MethodInfo methodInfo, string name)
        {
            Name = NamingHelper.ConvertToCamelCase(name);
            Type = methodInfo.ReturnType;
            Method = methodInfo;
            Weight = Method.GetParameters().Length;
            IsConstructorType = false;
        }

        public InstantiateModel(Type type)
            : this(type, type.Name)
        {

        }

        public InstantiateModel(MethodInfo methodInfo)
            : this(methodInfo, NamingHelper.ConvertToCamelCase(methodInfo.Name))
        {
            
        }

        public void UpdateWeight(int weight)
        {
            Weight = weight;
        }
    }
}

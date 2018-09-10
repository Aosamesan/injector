using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Injector.Helpers;

namespace Injector.ContextLoader.Models
{
    public sealed class LoadableTypeModel
    {
        public bool IsConstructorType { get; }
        public string Name { get; }
        public Type Type { get; }
        public int Weight { get; private set; }
        public MethodBase Method { get; }

        public LoadableTypeModel(Type type, string name)
        {
            Name = NamingHelper.ConvertToCamelCase(name);
            Type = type;
            Method = type.GetConstructors()[0];
            Weight = Method.GetParameters().Length;
            IsConstructorType = true;
        }

        public LoadableTypeModel(MethodInfo methodInfo, string name)
        {
            Name = NamingHelper.ConvertToCamelCase(name);
            Type = methodInfo.ReturnType;
            Method = methodInfo;
            Weight = Method.GetParameters().Length;
            IsConstructorType = false;
        }

        public LoadableTypeModel(Type type)
            : this(type, type.Name)
        {

        }

        public LoadableTypeModel(MethodInfo methodInfo)
            : this(methodInfo, NamingHelper.ConvertToCamelCase(methodInfo.Name))
        {
            
        }

        public void UpdateWeight(int weight)
        {
            Weight = weight;
        }
    }
}

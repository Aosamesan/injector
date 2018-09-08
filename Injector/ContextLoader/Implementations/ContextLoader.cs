using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Injector.Context;
using Injector.Context.Attributes;
using Injector.Context.Implementations;
using Injector.Helpers;

namespace Injector.ContextLoader.Implementations
{
    public class ContextLoader : ILoader
    {
        [ContextConfiguableMethod]
        public IContext LoadContext(Type contextType)
        {
            // Check a context class is marked with the [Context] attribute.
            AttributeHelper.CheckCorrectContext<Context.Attributes.Context>(contextType);
            // Check a context class is instantiable
            ReflectionHelper.CheckInstantiableClass(contextType);
            
            var contextBuilder = new InjectorContext.InjectorContextBuilder();
            var contextInstance = Activator.CreateInstance(contextType);

            // Create Objects
            ////// 1. methods in configuration without parameter
            foreach (var methodInfo in contextType.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!AttributeHelper.IsMethodMarked<Instantiate>(methodInfo) ||
                    methodInfo.GetParameters()?.Length != 0) continue;
                var result = methodInfo.Invoke(contextInstance, null);
                var name = NamingHelper.ConvertToCamelCase(methodInfo.Name);
                contextBuilder.Add(name, result);
            }
            
            ////// 2. scannable classes without constructor's parameter
            var namespaceToScan = AttributeHelper.GetNamespaceToScan(contextType);
            var currentAssembly = Assembly.GetExecutingAssembly();
            var modulesToScan = currentAssembly.GetModules().Find(module => namespaceToScan.Contains(module.Name)); 

            foreach (var module in modulesToScan)
            {
                var scannableClasses = module.GetTypes().Find(type =>
                    AttributeHelper.IsTypeMarked<Scannable>(type) &&
                    ReflectionHelper.HasParameterlessPublicConstructor(type)); 
                    
                foreach (var scannableClass in scannableClasses)
                {
                    var result = Activator.CreateInstance(scannableClass);
                    var name = NamingHelper.ConvertToCamelCase(scannableClass.Name);
                    contextBuilder.Add(name, result);
                }
            }
            
            ////// 3. methods in configuration with parameters
            foreach (var methodInfo in contextType.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                if (AttributeHelper.IsMethodMarked<Instantiate>(methodInfo) && methodInfo.GetParameters()?.Length == 0)
                {
                    IEnumerable<ParameterInfo> parameterInfos = methodInfo.GetParameters();
                    var parameterObjectList = new List<object>();

                    foreach (var parameterInfo in parameterInfos)
                    {
                        if (AttributeHelper.IsParameterMarked<Autowired>(parameterInfo))
                        {
                            var autowiredAttribute = parameterInfo.GetCustomAttribute(typeof(Autowired)) as Autowired;
                            var parameterType = parameterInfo.ParameterType;
                            var parameterName = autowiredAttribute?.Name;
                            object parameter = null;

                            if (!string.IsNullOrEmpty(parameterName))
                            {
                                if (!contextBuilder.Contains(parameterName))
                                {
                                    parameterName = NamingHelper.ConvertToCamelCase(parameterInfo.Name);
                                }
                                parameter = contextBuilder.Get(parameterName);
                            }
                            else
                            {
                                parameter = contextBuilder.Get(parameterType);
                            }


                            if (parameter == null)
                            {
                                throw new Exception();
                            }

                            parameterObjectList.Add(parameter);
                        }
                    }

                    var parameters = parameterObjectList.ToArray();
                    var result = methodInfo.Invoke(contextInstance, parameters);
                    var name = NamingHelper.ConvertToCamelCase(methodInfo.Name);
                    contextBuilder.Add(name, result);
                }
            }
            
            ////// 4. scannable classes with constructor's parameters TODO : implements
            
            
            // field inject TODO : implements

            return contextBuilder.Build();
        }
    }
}
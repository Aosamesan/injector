using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using Injector.Context;
using Injector.Context.Attributes;
using Injector.Context.Implementations;
using Injector.ContextLoader.HelperClass;
using Injector.ContextLoader.Models;
using Injector.Helpers;

namespace Injector.ContextLoader
{
    public class GenericContextLoader
    {
        private Type ContextType { get; }
        private object Context { get; }
        private IDictionary<string, LoadableTypeModel> ModelStorage { get; }
        private IDictionary<string, object> ObjectStorage { get; }

        private GenericContextLoader(Type contextType)
        {
            ContextType = contextType;
            Context = Activator.CreateInstance(ContextType);
            ModelStorage = new Dictionary<string, LoadableTypeModel>();
            ObjectStorage = new Dictionary<string, object>();
        }

        public static GenericContextLoader CreateContextLoader<T>()
        {
            var contextType = typeof(T);
            // Check a context class is marked with the [Context] attribute.
            AttributeHelper.CheckCorrectContext<Context.Attributes.Context>(contextType);
            // Check a context class is instantiable
            ReflectionHelper.CheckInstantiableClass(contextType);
            return new GenericContextLoader(contextType);
        }

        private LoadableTypeModel AddModel(Type type)
        {
            return AddModel(new LoadableTypeModel(type));
        }

        private LoadableTypeModel AddModel(MethodInfo methodInfo)
        {
            return AddModel(new LoadableTypeModel(methodInfo));
        }

        private LoadableTypeModel AddModel(Type type, string name)
        {
            return AddModel(new LoadableTypeModel(type, name));
        }

        private LoadableTypeModel AddModel(MethodInfo methodInfo, string name)
        {
            return AddModel(new LoadableTypeModel(methodInfo, name));
        }

        private LoadableTypeModel AddModel(LoadableTypeModel model)
        {
            return ModelStorage[model.Name] = model;
        }

        private object Instantiate(LoadableTypeModel model)
        {
            var argumentList = from string name
                    in ObjectStorage.Keys
                               where (from ParameterInfo paramInfo
                                           in model.Method.GetParameters()
                                      select IsEqualName(name, paramInfo)
                                   ).Aggregate(false, CommonHelper.Disjunction)
                               select ObjectStorage[name];

            if (model.IsConstructorType)
            {
                return Activator.CreateInstance(model.Type, argumentList.ToArray());
            }
            else
            {
                return model.Method.Invoke(Context, argumentList.ToArray());
            }
        }

        private void UpdateWeight(LoadableTypeModel model)
        {
            var weight = (from string name
                        in ModelStorage.Keys
                          where (from ParameterInfo paramInfo
                                      in model.Method.GetParameters()
                                 select IsEqualName(name, paramInfo)
                              ).Aggregate(false, CommonHelper.Disjunction)
                          select ModelStorage[name].Weight
                ).Aggregate(0, (a, b) => a + b);
            model.UpdateWeight(weight);
        }

        private bool CanInstantiate(LoadableTypeModel model)
        {
            var parameterInfos = model.Method.GetParameters();
            var required = parameterInfos.Length;
            var complete = (from string name
                        in ObjectStorage.Keys
                            where (from ParameterInfo paramInfo
                                        in parameterInfos
                                   select IsEqualName(name, paramInfo)
                                ).Aggregate(false, CommonHelper.Disjunction)
                            select ModelStorage[name]
                ).Count();
            return required == complete;
        }

        private bool IsEqualName(string name, ParameterInfo parameterInfo)
        {
            var attr = parameterInfo.GetCustomAttribute<Autowired>();
            if (!string.IsNullOrEmpty(attr?.Name))
            {
                return name == attr.Name;
            }

            return name == parameterInfo.Name;
        }

        [ContextConfiguableMethod]
        public IContext LoadContext()
        {
            var contextBuilder = new InjectorContext.InjectorContextBuilder();
            // initialize models
            var priorityQueue = new SimplePriorityQueue<int, LoadableTypeModel>();
            var methodsInContext = from MethodInfo methodInfo
                    in ContextType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                                   where AttributeHelper.IsMethodMarked<Instantiate>(methodInfo)
                                   select methodInfo;

            foreach (var methodInfo in methodsInContext)
            {
                var attr = methodInfo.GetCustomAttribute<Instantiate>();
                var model = string.IsNullOrEmpty(attr.Name) ? AddModel(methodInfo) : AddModel(methodInfo, attr.Name);

                if (model != null)
                {
                    priorityQueue.Enqueue(model.Weight, model);

                }
            }

            var namespaceToScan = AttributeHelper.GetNamespaceToScan(ContextType);
            var assembly = Assembly.GetCallingAssembly();

            var scanTypes = from Type type
                    in assembly.GetTypes()
                            where AttributeHelper.IsTypeMarked<Instantiate>(type) && namespaceToScan.Contains(type.Namespace)
                            select type;
            foreach (var scanType in scanTypes)
            {
                var attr = scanType.GetCustomAttribute<Instantiate>();
                var model = string.IsNullOrEmpty(attr.Name) ? AddModel(scanType) : AddModel(scanType, attr.Name);

                if (model != null)
                {
                    priorityQueue.Enqueue(model.Weight, model);
                }
            }

            // Create Objects
            int previousCount = priorityQueue.Count;

            while (!priorityQueue.IsEmpty)
            {
                var model = priorityQueue.Dequeue();
                if (CanInstantiate(model))
                {
                    var instance = Instantiate(model);
                    ObjectStorage[model.Name] = instance;
                }
                else
                {
                    UpdateWeight(model);
                    priorityQueue.Enqueue(model.Weight, model);
                }

                if (previousCount <= priorityQueue.Count)
                {
                    throw new Exception("Error Occurred");
                }

                previousCount = priorityQueue.Count;
            }

            // Property Injection
            foreach (var instance in ObjectStorage.Values)
            {
                var instanceType = instance.GetType();
                var properties = from PropertyInfo propertyInfo
                        in instanceType.GetProperties()
                                 where AttributeHelper.IsPropertyMarked<Autowired>(propertyInfo)
                                 select propertyInfo;
                foreach (var property in properties)
                {
                    var attr = property.GetCustomAttribute<Autowired>();
                    if (!string.IsNullOrEmpty(attr?.Name))
                    {
                        property.SetValue(instance, ObjectStorage[attr.Name]);
                    }
                }
            }

            // Build
            foreach (var pair in ObjectStorage)
            {
                contextBuilder.Add(pair.Key, pair.Value);
            }

            return contextBuilder.Build();
        }
    }
}
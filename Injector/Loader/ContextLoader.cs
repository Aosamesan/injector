using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Injector.Context;
using Injector.Context.Attributes;
using Injector.Context.Exceptions;
using Injector.Context.Implementations;
using Injector.Helpers;
using Injector.Loader.Exceptions;
using Injector.Loader.HelperClass;
using Injector.Loader.Models;
using Injector.PropertyLoader.Attributes;
using Injector.PropertyLoader.Loader;

namespace Injector.Loader
{
    public class ContextLoader
    {
        private Type ContextType { get; }
        private object Context { get; }
        private IDictionary<string, InstantiateModel> ModelStorage { get; }
        private IDictionary<string, object> ObjectStorage { get; }
        private PropertyLoader.Loader.PropertyLoader PropertyLoader { get; }

        private ContextLoader(Type contextType)
        {
            ContextType = contextType;
            Context = Activator.CreateInstance(ContextType);
            ModelStorage = new Dictionary<string, InstantiateModel>();
            ObjectStorage = new Dictionary<string, object>();
            PropertyLoader = new PropertyLoader.Loader.PropertyLoader();
        }

        public static ContextLoader CreateContextLoader<T>()
        {
            var contextType = typeof(T);
            // Check a context class is marked with the [Context] attribute.
            AttributeHelper.CheckHasTypeAttribute<Context.Attributes.Context, NotLoadableContextException>(contextType);
            // Check a context class is instantiable
            ReflectionHelper.CheckInstantiableClass(contextType);
            return new ContextLoader(contextType);
        }

        private InstantiateModel AddModel(Type type)
        {
            return AddModel(new InstantiateModel(type));
        }

        private InstantiateModel AddModel(MethodInfo methodInfo)
        {
            return AddModel(new InstantiateModel(methodInfo));
        }

        private InstantiateModel AddModel(Type type, string name)
        {
            return AddModel(new InstantiateModel(type, name));
        }

        private InstantiateModel AddModel(MethodInfo methodInfo, string name)
        {
            return AddModel(new InstantiateModel(methodInfo, name));
        }

        private InstantiateModel AddModel(InstantiateModel model)
        {
            return ModelStorage[model.Name] = model;
        }

        private object Instantiate(InstantiateModel model)
        {
            var argumentList = from string name
                    in ObjectStorage.Keys
                               where (from ParameterInfo paramInfo
                                           in model.Method.GetParameters()
                                      select IsEqualName(name, paramInfo)
                                   ).Aggregate(false, CommonHelper.Disjunction)
                               select ObjectStorage[name];

            return model.IsConstructorType ? Activator.CreateInstance(model.Type, argumentList.ToArray()) : model.Method.Invoke(Context, argumentList.ToArray());
        }

        private void UpdateWeight(InstantiateModel model)
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

        private bool CanInstantiate(InstantiateModel model)
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
            var propertyDictionary = PropertyLoader.Load(ContextType);
            // inject property into context
            var contextProperties = from PropertyInfo propertyInfo
                    in ContextType.GetProperties()
                where propertyInfo.PropertyType == typeof(string) &&
                      AttributeHelper.IsPropertyMarked<PropertyValue>(propertyInfo)
                select (propertyInfo, AttributeHelper.getAttribute<PropertyValue>(propertyInfo));

            foreach (var property in contextProperties)
            {
                property.Item1.SetValue(Context, propertyDictionary[property.Item2.Name]);
            }
            
            var contextBuilder = new InjectorContext.InjectorContextBuilder();
            // initialize models
            var priorityQueue = new SimplePriorityQueue<int, InstantiateModel>();
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
            
            // create models to instantiate
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
            var previousCount = priorityQueue.Count;

            while (!priorityQueue.IsEmpty)
            {
                var model = priorityQueue.Dequeue();
                if (CanInstantiate(model))
                {
                    var instance = Instantiate(model);
                    // property injection
                    if (model.IsConstructorType)
                    {
                        var properties = from PropertyInfo propertyInfo in model.Type.GetProperties()
                            where AttributeHelper.IsPropertyMarked<PropertyValue>(propertyInfo)
                            select (propertyInfo, AttributeHelper.getAttribute<PropertyValue>(propertyInfo));

                        foreach (var property in properties)
                        {
                            var name = property.Item2.Name;
                            property.Item1.SetValue(instance, propertyDictionary[name]);
                        }
                    }
                    ObjectStorage[model.Name] = instance;
                }
                else
                {
                    UpdateWeight(model);
                    priorityQueue.Enqueue(model.Weight, model);
                }

                if (previousCount <= priorityQueue.Count)
                {
                    throw new DependencyNotFoundException(model);
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
                    property.SetValue(instance,
                        !string.IsNullOrEmpty(attr?.Name)
                            ? ObjectStorage[attr.Name]
                            : ObjectStorage[NamingHelper.ConvertToCamelCase(property.Name)]);
                }
            }
            
            // invoke post constructor
            foreach (var name in ObjectStorage.Keys)
            {
                var model = ModelStorage[name];
                if (!model.IsConstructorType)
                    continue;
                var type = model.Type;
                var instance = ObjectStorage[name];
                var postConstructors =
                    from MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    where AttributeHelper.IsMethodMarked<PostConsturctor>(method)
                    select method;
                foreach (var postConstructor in postConstructors)
                {
                    postConstructor.Invoke(instance, null);
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
using System;
using System.Collections.Generic;
using StraightInject.Core.ConstructorResolver;
using StraightInject.Core.Services;
using StraightInject.Services;

namespace StraightInject.Core.ComponentComposers
{
    /// <summary>
    /// Used to compose a specific type component
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class TypedComponentComposer<T> : IComponentComposer<IConstructableService, T>
    {
        private readonly Dictionary<Type, IService> dependencies;

        public TypedComponentComposer(Dictionary<Type, IService> dependencies)
        {
            this.dependencies = dependencies;
        }

        public IConstructableService ToService<TService>()
        {
            var typedService = new TypedService(typeof(T), new EagerConstructorResolver(), typeof(TService));
            dependencies.Add(typeof(TService),
                typedService);

            return typedService;
            ;
        }

        public IConstructableService ToService(Type serviceType)
        {
            var typedService = new TypedService(typeof(T), new EagerConstructorResolver(), serviceType);
            dependencies.Add(serviceType, typedService);

            return typedService;
        }
    }

    internal class TypedComponentComposer : IComponentComposer<IConstructableService>
    {
        private readonly Type componentType;
        private readonly Dictionary<Type, IService> dependencies;

        public TypedComponentComposer(Type componentType, Dictionary<Type, IService> dependencies)
        {
            this.componentType = componentType;
            this.dependencies = dependencies;
        }

        public IConstructableService ToService<TService>()
        {
            var typedService = new TypedService(componentType, new EagerConstructorResolver(), typeof(TService));
            dependencies.Add(typeof(TService), typedService);

            return typedService;
        }

        public IConstructableService ToService(Type serviceType)
        {
            var typedService = new TypedService(componentType, new EagerConstructorResolver(), serviceType);
            dependencies.Add(serviceType, typedService);

            return typedService;
        }
    }
}
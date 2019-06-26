using System;
using System.Collections.Generic;
using StraightInject.Core.ConstructorResolver;
using StraightInject.Core.Services;
using StraightInject.Services;

namespace StraightInject.Core.ComponentComposers
{
    internal class SingleInstanceComponentComposer<T> : IComponentComposer<IConstructableService, T>
    {
        private readonly Dictionary<Type, IService> dependencies;

        public SingleInstanceComponentComposer(Dictionary<Type, IService> dependencies)
        {
            this.dependencies = dependencies;
        }

        public IConstructableService ToService<TServiceType>()
        {
            var typedService = new SingletonService(typeof(T), new EagerConstructorResolver(), typeof(TServiceType));
            dependencies.Add(typeof(TServiceType),
                typedService);

            return typedService;
        }

        public IConstructableService ToService(Type serviceType)
        {
            var typedService = new SingletonService(typeof(T), new EagerConstructorResolver(), serviceType);
            dependencies.Add(serviceType, typedService);

            return typedService;
        }
    }

    internal class SingleInstanceComponentComposer : IComponentComposer<IConstructableService>
    {
        private readonly Type componentType;
        private readonly Dictionary<Type, IService> dependencies;

        public SingleInstanceComponentComposer(Type componentType, Dictionary<Type, IService> dependencies)
        {
            this.componentType = componentType;
            this.dependencies = dependencies;
        }

        public IConstructableService ToService<TService>()
        {
            var typedService = new SingletonService(componentType, new EagerConstructorResolver(), typeof(TService));
            dependencies.Add(typeof(TService), typedService);

            return typedService;
        }

        public IConstructableService ToService(Type serviceType)
        {
            var typedService = new SingletonService(componentType, new EagerConstructorResolver(), serviceType);
            dependencies.Add(serviceType, typedService);

            return typedService;
        }
    }
}
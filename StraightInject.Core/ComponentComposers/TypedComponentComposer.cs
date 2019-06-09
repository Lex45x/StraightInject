using System;
using System.Collections.Generic;
using StraightInject.Core.ConstructorResolver;
using StraightInject.Core.Services;

namespace StraightInject.Core.ComponentComposers
{
    internal class TypedComponentComposer<T> : IComponentComposer<T>
    {
        private readonly Dictionary<Type, IService> dependencies;

        public TypedComponentComposer(Dictionary<Type, IService> dependencies)
        {
            this.dependencies = dependencies;
        }

        public void ToService<TService>()
        {
            dependencies.Add(typeof(TService), new TypedService(typeof(T), new EagerConstructorResolver(), typeof(TService)));
        }

        public void ToService(Type serviceType)
        {
            dependencies.Add(serviceType, new TypedService(typeof(T), new EagerConstructorResolver(), serviceType));
        }
    }

    internal class TypedComponentComposer : IComponentComposer
    {
        private readonly Type componentType;
        private readonly Dictionary<Type, IService> dependencies;

        public TypedComponentComposer(Type componentType, Dictionary<Type, IService> dependencies)
        {
            this.componentType = componentType;
            this.dependencies = dependencies;
        }

        public void ToService<TService>()
        {
            dependencies.Add(typeof(TService), new TypedService(componentType, new EagerConstructorResolver(), typeof(TService)));
        }

        public void ToService(Type serviceType)
        {
            dependencies.Add(serviceType, new TypedService(componentType, new EagerConstructorResolver(), serviceType));
        }
    }
}
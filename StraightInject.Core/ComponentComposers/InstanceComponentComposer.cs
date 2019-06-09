using System;
using System.Collections.Generic;
using StraightInject.Core.Services;

namespace StraightInject.Core.ComponentComposers
{
    internal class InstanceComponentComposer<T> : IComponentComposer<T>
    {
        private readonly Dictionary<Type, IService> dependencies;
        private readonly T instance;

        public InstanceComponentComposer(Dictionary<Type, IService> dependencies, T instance)
        {
            this.dependencies = dependencies;
            this.instance = instance;
        }

        public void ToService<TService>()
        {
            dependencies.Add(typeof(TService), new InstanceService(instance, typeof(TService)));
        }

        public void ToService(Type serviceType)
        {
            dependencies.Add(serviceType, new InstanceService(instance, serviceType));
        }
    }
}
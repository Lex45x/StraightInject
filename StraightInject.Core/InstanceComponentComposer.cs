using System;
using System.Collections.Generic;

namespace StraightInject.Core
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
            dependencies.Add(typeof(TService), new SingletonService(instance));
        }

        public void ToService(Type serviceType)
        {
            dependencies.Add(serviceType, new SingletonService(instance));
        }
    }
}
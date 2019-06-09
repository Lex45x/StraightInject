using System;
using System.Collections.Generic;

namespace StraightInject.Core
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
            dependencies.Add(typeof(TService), new TypedService(typeof(T)));
        }

        public void ToService(Type serviceType)
        {
            dependencies.Add(serviceType, new TypedService(typeof(T)));
        }
    }
}
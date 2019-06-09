using System;
using System.Collections.Generic;

namespace StraightInject.Core
{
    internal class TypeComponentComposer : IComponentComposer
    {
        private readonly Type componentType;
        private readonly Dictionary<Type, IService> dependencies;

        public TypeComponentComposer(Type componentType, Dictionary<Type, IService> dependencies)
        {
            this.componentType = componentType;
            this.dependencies = dependencies;
        }

        public void ToService<TService>()
        {
            dependencies.Add(typeof(TService), new TypedService(componentType));
        }

        public void ToService(Type serviceType)
        {
            dependencies.Add(serviceType, new TypedService(componentType));
        }
    }
}
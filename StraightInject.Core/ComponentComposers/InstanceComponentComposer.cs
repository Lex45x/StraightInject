using System;
using System.Collections.Generic;
using StraightInject.Core.Services;
using StraightInject.Services;

namespace StraightInject.Core.ComponentComposers
{
    internal class InstanceComponentComposer<TInstance> : IComponentComposer<IService, TInstance>
    {
        private readonly Dictionary<Type, IService> dependencies;
        private readonly TInstance instance;

        public InstanceComponentComposer(Dictionary<Type, IService> dependencies, TInstance instance)
        {
            this.dependencies = dependencies;
            this.instance = instance;
        }

        public IService ToService<TService>()
        {
            var instanceService = new InstanceService(instance, typeof(TService));
            dependencies.Add(typeof(TService), instanceService);
            return instanceService;
        }

        public IService ToService(Type serviceType)
        {
            var instanceService = new InstanceService(instance, serviceType);
            dependencies.Add(serviceType, instanceService);
            return instanceService;
        }
    }
}
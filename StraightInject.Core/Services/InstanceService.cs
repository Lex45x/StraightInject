using System;
using StraightInject.Services;

namespace StraightInject.Core.Services
{
    internal class InstanceService : IService
    {
        public object Instance { get; }

        public InstanceService(object instance, Type serviceType)
        {
            Instance = instance;
            ServiceType = serviceType;
        }

        public Type ServiceType { get; }
    }
}
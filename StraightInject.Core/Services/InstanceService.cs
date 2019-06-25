using System;
using StraightInject.Services;

namespace StraightInject.Core.Services
{
    /// <summary>
    /// Service provided by already created instance
    /// </summary>
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
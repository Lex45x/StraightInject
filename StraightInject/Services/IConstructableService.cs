using System;
using System.Collections.Generic;
using System.Reflection;

namespace StraightInject.Services
{
    /// <summary>
    /// Wrap a service that must be instantiated explicitly
    /// </summary>
    public interface IConstructableService : IService
    {
        ConstructorInfo GetConstructor(Dictionary<Type, IService> dependencies);
        void OverrideConstructorResolver(IConstructorResolver resolver);
    }
}
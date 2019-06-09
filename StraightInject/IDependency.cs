using System;
using System.Collections.Generic;
using System.Reflection;

namespace StraightInject
{
    public interface IService
    {
        Type ServiceType { get; }
    }

    public interface IConstructableService : IService
    {
        ConstructorInfo GetConstructor(Dictionary<Type, IService> dependencies);
        void OverrideConstructorResolver(IConstructorResolver resolver);
    }
}
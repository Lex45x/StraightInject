using System;
using System.Collections.Generic;
using System.Reflection;

namespace StraightInject.Services
{
    public interface IConstructableService : IService
    {
        ConstructorInfo GetConstructor(Dictionary<Type, IService> dependencies);
        void OverrideConstructorResolver(IConstructorResolver resolver);
    }
}
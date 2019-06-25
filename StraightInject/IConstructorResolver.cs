using System;
using System.Collections.Generic;
using System.Reflection;
using StraightInject.Services;

namespace StraightInject
{
    /// <summary>
    /// Add ability to resolve a constructor for a specific component
    /// </summary>
    public interface IConstructorResolver
    {
        ConstructorInfo Resolve(Type component, Dictionary<Type, IService> dependencies);
    }
}
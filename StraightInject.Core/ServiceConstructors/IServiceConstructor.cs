using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using StraightInject.Services;

namespace StraightInject.Core.ServiceConstructors
{
    internal interface IServiceCompiler
    {
        Action<ILGenerator> Construct(Type flatContainer, IService service,
            Dictionary<Type, Action<ILGenerator>> knownTypes,
            Dictionary<Type, IService> dependencies);
    }
}
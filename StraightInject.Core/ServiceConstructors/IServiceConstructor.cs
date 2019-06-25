using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using StraightInject.Core.Compilers;
using StraightInject.Services;

namespace StraightInject.Core.ServiceConstructors
{
    /// <summary>
    /// Compile a service into a set of IL instructions that will be applied with Action delegate
    /// </summary>
    internal interface IServiceCompiler
    {
        Action<ILGenerator> Compile(Type flatContainer, IService service,
            Dictionary<Type, Action<ILGenerator>> knownTypes,
            Dictionary<Type, IService> dependencies, IContainerInitialState initialState, FieldInfo stateField);
    }
}
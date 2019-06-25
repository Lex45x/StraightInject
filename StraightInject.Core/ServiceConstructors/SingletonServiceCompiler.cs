using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using StraightInject.Core.Compilers;
using StraightInject.Services;

namespace StraightInject.Core.ServiceConstructors
{
    /// <summary>
    /// Provide a compilation for singleton service (instance is not created yet)
    /// </summary>
    internal class SingletonServiceCompiler : TypedServiceCompiler
    {
        public override Action<ILGenerator> Compile(Type flatContainer, IService service,
            Dictionary<Type, Action<ILGenerator>> knownTypes, Dictionary<Type, IService> dependencies,
            IContainerInitialState initialState, FieldInfo stateField)
        {
            var action = base.Compile(flatContainer, service, knownTypes, dependencies, initialState, stateField);

            throw new NotImplementedException();

            return action;
        }
    }
}
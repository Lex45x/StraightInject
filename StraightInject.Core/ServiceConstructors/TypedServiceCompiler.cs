using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using StraightInject.Core.Compilers;
using StraightInject.Core.Services;
using StraightInject.Services;

namespace StraightInject.Core.ServiceConstructors
{
    /// <summary>
    /// Compile a service of a specific type
    /// </summary>
    internal class TypedServiceCompiler : IServiceCompiler
    {
        public virtual Action<ILGenerator> Compile(Type flatContainer, IService service,
            Dictionary<Type, Action<ILGenerator>> knownTypes,
            Dictionary<Type, IService> dependencies, IContainerInitialState initialState, FieldInfo stateField)
        {
            if (knownTypes.ContainsKey(service.ServiceType))
            {
                return knownTypes[service.ServiceType];
            }

            if (!(service is TypedService typedService))
            {
                throw new InvalidOperationException(
                    $"Invalid TypedServiceCompiler usage on Non-TypedService. Original service: {service.GetType().FullName}");
            }

            var constructor = typedService.GetConstructor(dependencies);

            void GeneratorAction(ILGenerator generator)
            {
                foreach (var parameterInfo in constructor.GetParameters())
                {
                    knownTypes[parameterInfo.ParameterType](generator);
                }

                generator.Emit(OpCodes.Newobj, constructor);
            }

            return GeneratorAction;
        }
    }
}
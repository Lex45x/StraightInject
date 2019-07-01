using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using StraightInject.Core.Debugging;
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
                DebugMode.Execute(() =>
                {
                    Console.WriteLine("[{0}] Type {1} already has compiled creation", GetType().Name, service.ServiceType.FullName);
                });

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
                DebugMode.Execute(() =>
                {
                    Console.WriteLine("[{0}] Compiling creation of object of type {1}", GetType().Name, service.ServiceType.FullName);
                });

                foreach (var parameterInfo in constructor.GetParameters())
                {
                    knownTypes[parameterInfo.ParameterType](generator);
                }

                generator.Emit(OpCodes.Newobj, constructor);

                DebugMode.Execute(() =>
                {
                    Console.WriteLine("[{0}] Compiled object creation of Type: {1} with parameters: {2}", GetType().Name, service.ServiceType.FullName,
                        constructor.GetParameters().Aggregate(new StringBuilder(),
                            (builder, info) => builder.Append(
                                $" Name: {info.Name}, Type: {info.ParameterType.FullName}")));
                });
            }

            return GeneratorAction;
        }
    }
}
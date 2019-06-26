using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using StraightInject.Core.Compilers;
using StraightInject.Core.Debugging;
using StraightInject.Core.Services;
using StraightInject.Services;

namespace StraightInject.Core.ServiceConstructors
{
    /// <summary>
    /// Provide a compilation for Instance specified service
    /// </summary>
    internal class InstanceServiceCompiler : IServiceCompiler
    {
        public Action<ILGenerator> Compile(Type flatContainer, IService service,
            Dictionary<Type, Action<ILGenerator>> knownTypes, Dictionary<Type, IService> dependencies,
            IContainerInitialState initialState, FieldInfo stateField)
        {
            if (knownTypes.ContainsKey(service.ServiceType))
            {
                DebugMode.Execute(() =>
                {
                    Console.WriteLine("[{0}] Instance of type {1} already have compiled getter", GetType().Name, service.ServiceType.FullName);
                });
                return knownTypes[service.ServiceType];
            }

            if (!(service is InstanceService instanceService))
            {
                throw new InvalidOperationException(
                    $"Invalid TypedServiceCompiler usage on Non-TypedService. Original service: {service.GetType().FullName}");
            }

            initialState.ComponentInstances.Add(instanceService.Instance.GetType(), instanceService.Instance);

            var getMethod = typeof(IContainerInitialState)
                .GetProperty("ComponentInstances", BindingFlags.Public | BindingFlags.Instance).GetMethod;

            var indexer = typeof(Dictionary<Type, object>).GetProperties().First(x => x.GetIndexParameters().Length > 0)
                .GetMethod;

            void ReturnInstance(ILGenerator generator)
            {
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldfld, stateField);

                generator.Emit(OpCodes.Callvirt, getMethod);

                generator.Emit(OpCodes.Ldtoken, instanceService.Instance.GetType());
                generator.Emit(OpCodes.Callvirt, indexer);
            }

            DebugMode.Execute(() =>
            {
                Console.WriteLine("[{0}] ILGenerator with getter for Instance of type {1} successfully created", GetType().Name, service.ServiceType.FullName);
            });

            return ReturnInstance;
        }

        
    }
}
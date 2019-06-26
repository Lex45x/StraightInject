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
    /// Provide a compilation for singleton service (instance is not created yet)
    /// </summary>
    internal class SingletonServiceCompiler : TypedServiceCompiler
    {
        public override Action<ILGenerator> Compile(Type flatContainer, IService service,
            Dictionary<Type, Action<ILGenerator>> knownTypes, Dictionary<Type, IService> dependencies,
            IContainerInitialState initialState, FieldInfo stateField)
        {
            if (!(service is SingletonService singletonService))
            {
                throw new InvalidOperationException(
                    $"Invalid TypedServiceCompiler usage on Non-TypedService. Original service: {service.GetType().FullName}");
            }

            var action = base.Compile(flatContainer, service, knownTypes, dependencies, initialState, stateField);


            var getMethod = typeof(IContainerInitialState)
                .GetProperty("ComponentInstances", BindingFlags.Public | BindingFlags.Instance).GetMethod;

            var containsKey =
                typeof(Dictionary<Type, object>).GetMethod("ContainsKey", BindingFlags.Public | BindingFlags.Instance);

            var indexer = typeof(Dictionary<Type, object>).GetProperties()
                .First(x => x.GetIndexParameters().Length > 0);

            void GeneratorAction(ILGenerator generator)
            {
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldfld, stateField);
                generator.Emit(OpCodes.Callvirt, getMethod);

                generator.Emit(OpCodes.Ldtoken, singletonService.OriginalType);
                generator.Emit(OpCodes.Callvirt, containsKey);

                var label = generator.DefineLabel();
                var finish = generator.DefineLabel();

                generator.Emit(OpCodes.Brfalse, label);

                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldfld, stateField);
                generator.Emit(OpCodes.Callvirt, getMethod);

                generator.Emit(OpCodes.Ldtoken, singletonService.OriginalType);
                generator.Emit(OpCodes.Callvirt, indexer.GetMethod);
                generator.Emit(OpCodes.Br, finish);

                generator.MarkLabel(label);

                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldfld, stateField);
                generator.Emit(OpCodes.Callvirt, getMethod);
                generator.Emit(OpCodes.Ldtoken, singletonService.OriginalType);
                action(generator);

                generator.Emit(OpCodes.Callvirt, indexer.SetMethod);

                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldfld, stateField);
                generator.Emit(OpCodes.Callvirt, getMethod);
                generator.Emit(OpCodes.Ldtoken, singletonService.OriginalType);
                generator.Emit(OpCodes.Callvirt, indexer.GetMethod);
                generator.MarkLabel(finish);
            }

            DebugMode.Execute(() =>
            {
                Console.WriteLine("[{0}] ILGenerator with getter for SingleInstance of type {1} successfully created", GetType().Name, service.ServiceType.FullName);
            });
            return GeneratorAction;
        }
    }
}
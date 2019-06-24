using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
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
            Dictionary<Type, Action<ILGenerator>> knownTypes, Dictionary<Type, IService> dependencies)
        {
            if (knownTypes.ContainsKey(service.ServiceType))
            {
                return knownTypes[service.ServiceType];
            }

            if (!(service is InstanceService instanceService))
            {
                throw new InvalidOperationException(
                    $"Invalid TypedServiceCompiler usage on Non-TypedService. Original service: {service.GetType().FullName}");
            }

            var instance = instanceService.Instance;


            void ReturnInstance(ILGenerator generator)
            {
                throw new NotImplementedException();
            }

            return ReturnInstance;
        }
    }

    /// <summary>
    /// Provide a compilation for singleton service (instance is not created yet)
    /// </summary>
    internal class SingletonServiceCompiler : TypedServiceCompiler
    {
        public override Action<ILGenerator> Compile(Type flatContainer, IService service,
            Dictionary<Type, Action<ILGenerator>> knownTypes, Dictionary<Type, IService> dependencies)
        {
            var action = base.Compile(flatContainer, service, knownTypes, dependencies);

            throw new NotImplementedException();

            return action;
        }
    }
}
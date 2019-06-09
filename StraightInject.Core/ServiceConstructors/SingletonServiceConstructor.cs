﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using StraightInject.Core.Services;
using StraightInject.Services;

namespace StraightInject.Core.ServiceConstructors
{
    internal class InstanceServiceCompiler : IServiceCompiler
    {
        public Action<ILGenerator> Construct(Type flatContainer, IService service,
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
                generator.Emit();
            }

            return ReturnInstance;
        }
    }

    internal class SingletonServiceCompiler : TypedServiceCompiler
    {
        public override Action<ILGenerator> Construct(Type flatContainer, IService service,
            Dictionary<Type, Action<ILGenerator>> knownTypes, Dictionary<Type, IService> dependencies)
        {
            var action = base.Construct(flatContainer, service, knownTypes, dependencies);
            
            return action;
        }
    }
}
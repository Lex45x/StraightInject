using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using StraightInject.Services;

namespace StraightInject.Core.ConstructorResolver
{
    public class EagerConstructorResolver : IConstructorResolver
    {
        public ConstructorInfo Resolve(Type component, Dictionary<Type, IService> dependencies)
        {
            var constructors =
                component.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

            foreach (var constructor in constructors.OrderByDescending(info => info.GetParameters().Length))
            {
                var allParametersAvailable = true;

                foreach (var parameterInfo in constructor.GetParameters())
                {
                    if (!dependencies.ContainsKey(parameterInfo.ParameterType))
                    {
                        allParametersAvailable = false;
                    }
                }

                if (allParametersAvailable)
                {
                    return constructor;
                }
            }

            throw new InvalidOperationException(
                $"Couldn't find a constructor that will fit to registered services");
        }
    }
}
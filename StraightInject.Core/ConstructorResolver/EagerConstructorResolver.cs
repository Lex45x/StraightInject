using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using StraightInject.Core.Debugging;
using StraightInject.Services;

namespace StraightInject.Core.ConstructorResolver
{
    /// <summary>
    /// Try to resolve a constructor with all presented params from most parameters descends to least.
    /// </summary>
    public class EagerConstructorResolver : IConstructorResolver
    {
        public ConstructorInfo Resolve(Type component, Dictionary<Type, IService> dependencies)
        {
            DebugMode.Execute(
                () =>
                {
                    Console.WriteLine("[{0}] Resolving constructor for type {1}",
                        GetType().Name, component.FullName);
                });
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
                    DebugMode.Execute(
                        () =>
                        {
                            Console.WriteLine("[{0}] Resolved constructor for type {1}{2}",
                                GetType().Name, component.FullName,
                                constructor.GetParameters()
                                    .Aggregate(new StringBuilder(constructor.GetParameters().Any()
                                            ? ", with params "
                                            : null),
                                        (builder, info) => builder.Append(
                                            $" Name: {info.Name}, Type: {info.ParameterType.FullName}")));
                        });
                    return constructor;
                }
            }

            throw new InvalidOperationException(
                $"Couldn't find a constructor that will fit to registered services");
        }
    }
}
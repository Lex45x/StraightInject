using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace StraightInject.Core
{
    internal class TypedServiceConstructor : IServiceConstructor
    {
        public Action<ILGenerator> Construct(Type serviceType, IService service,
            Dictionary<Type, Action<ILGenerator>> knownTypes, Dictionary<Type, IService> dependencies,
            Stack<Type> constructionStack)
        {
            if (constructionStack.Contains(serviceType))
            {
                throw new InvalidOperationException(
                    $"Recursive service detected for service {serviceType.FullName}{constructionStack.Aggregate(new StringBuilder(), (builder, type) => builder.Append(" => ").Append(type.FullName))}");
            }

            if (knownTypes.ContainsKey(serviceType))
            {
                return knownTypes[serviceType];
            }

            constructionStack.Push(serviceType);

            if (!(service is TypedService typeDependency))
            {
                throw new InvalidOperationException(
                    $"Invalid TypedServiceConstructor usage on Non-TypedService. Original service: {service.GetType().FullName}");
            }

            var constructors =
                typeDependency.OriginalType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

            ConstructorInfo eagerConstructor = null;

            foreach (var constructor in constructors.OrderByDescending(info => info.GetParameters().Length))
            {
                var allParametersAvailable = true;

                foreach (var parameterInfo in constructor.GetParameters())
                {
                    if (knownTypes.ContainsKey(parameterInfo.ParameterType)) continue;

                    if (!dependencies.TryGetValue(parameterInfo.ParameterType, out var childDependency))
                    {
                        allParametersAvailable = false;
                        continue;
                    }

                    Construct(parameterInfo.ParameterType,
                        childDependency,
                        knownTypes,
                        dependencies,
                        constructionStack);
                }

                if (!allParametersAvailable) continue;

                eagerConstructor = constructor;
                break;
            }

            if (eagerConstructor == null)
            {
                throw new NotImplementedException(
                    $"Couldn't find a constructor that will fit to registered services");
            }

            void GeneratorAction(ILGenerator generator)
            {
                foreach (var parameterInfo in eagerConstructor.GetParameters())
                {
                    knownTypes[parameterInfo.ParameterType](generator);
                }

                generator.Emit(OpCodes.Newobj, eagerConstructor);
            }

            constructionStack.Pop();

            knownTypes.Add(serviceType, GeneratorAction);

            return GeneratorAction;
        }
    }

    internal class SingletonServiceConstructor : IServiceConstructor
    {
        public Action<ILGenerator> Construct(Type serviceType, IService service,
            Dictionary<Type, Action<ILGenerator>> knownTypes, Dictionary<Type, IService> dependencies,
            Stack<Type> constructionStack)
        {
            throw new NotImplementedException();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace StraightInject.Core
{
    internal class TypeDependencyConstructor : IDependencyConstructor
    {
        public Action<ILGenerator> Construct(Type serviceType, IDependency dependency,
            Dictionary<Type, Action<ILGenerator>> knownTypes, Dictionary<Type, IDependency> dependencies,
            Stack<Type> constructionStack)
        {
            if (constructionStack.Contains(serviceType))
            {
                throw new InvalidOperationException(
                    $"Recursive dependency detected for service {serviceType.FullName}{constructionStack.Aggregate(new StringBuilder(), (builder, type) => builder.Append(" => ").Append(type.FullName))}");
            }

            if (knownTypes.ContainsKey(serviceType))
            {
                return knownTypes[serviceType];
            }

            constructionStack.Push(serviceType);

            if (!(dependency is TypeDependency typeDependency))
            {
                throw new InvalidOperationException(
                    $"Invalid TypeDependencyConstructor usage on Non-TypeDependency. Original dependency: {dependency.GetType().FullName}");
            }

            var constructors =
                typeDependency.OriginalType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

            var eagerConstructor = constructors.OrderByDescending(info => info.GetParameters().Length).First();

            foreach (var parameterInfo in eagerConstructor.GetParameters())
            {
                if (knownTypes.ContainsKey(parameterInfo.ParameterType)) continue;

                if (!dependencies.TryGetValue(parameterInfo.ParameterType, out var childDependency))
                {
                    throw new NotImplementedException(
                        $"Service {parameterInfo.ParameterType.FullName} from {typeDependency.OriginalType.FullName} eager constructor has no implementation provided.");
                }

                Construct(parameterInfo.ParameterType,
                    childDependency,
                    knownTypes,
                    dependencies,
                    constructionStack);
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
}
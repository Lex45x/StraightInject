using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using DynamicContainer;
using Lokad.ILPack;

namespace StraightInject.Core
{
    internal class DynamicAssemblyContainerCompiler : IContainerCompiler
    {
        private readonly Dictionary<Type, IDependencyConstructor> dependencyConstructors;

        private readonly ModuleBuilder dynamicModule;
        private readonly AssemblyBuilder assembly;

        public DynamicAssemblyContainerCompiler(Dictionary<Type, IDependencyConstructor> dependencyConstructors)
        {
            this.dependencyConstructors = dependencyConstructors;
            var assemblyName =
                new AssemblyName(
                    "DynamicContainer");
            assembly = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            dynamicModule =
                assembly.DefineDynamicModule(
                    "DynamicContainer");
        }

        public IContainer CompileDependencies(Dictionary<Type, IDependency> dependencies)
        {
            var knownTypes = GenerateIlAppenders(dependencies);

            var flatContainer = GenerateFlatContainer();

            var resolveMethod = AppendResolveMethod(flatContainer, knownTypes);

            var type = flatContainer.CreateTypeInfo();

            return Activator.CreateInstance(type) as IContainer;
        }

        private Dictionary<Type, Action<ILGenerator>> GenerateIlAppenders(Dictionary<Type, IDependency> dependencies)
        {
            var knownTypes = new Dictionary<Type, Action<ILGenerator>>();

            foreach (var (key, value) in dependencies)
            {
                dependencyConstructors[value.GetType()]
                    .Construct(key, value, knownTypes, dependencies, new Stack<Type>());
            }

            return knownTypes;
        }

        private MethodBuilder AppendResolveMethod(TypeBuilder flatContainer,
            Dictionary<Type, Action<ILGenerator>> knownTypes)
        {
            var resolveMethod = typeof(IContainer).GetMethod("Resolve",
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            var parameters = resolveMethod.GetParameters().Select(info => info.ParameterType).ToArray();

            var methodBuilder = flatContainer.DefineMethod(resolveMethod.Name,
                MethodAttributes.Public |
                MethodAttributes.Virtual |
                MethodAttributes.HideBySig |
                MethodAttributes.NewSlot |
                MethodAttributes.Final, resolveMethod.ReturnType,
                parameters);

            var genericParameters = methodBuilder.DefineGenericParameters("T");
            Type serviceTypeParameter = genericParameters.First();

            var ilGenerator = methodBuilder.GetILGenerator();

            var getType = typeof(Type).GetMethod("GetTypeFromHandle", BindingFlags.Public | BindingFlags.Static);
            var equalityOperator = typeof(Type).GetMethod("op_Equality", BindingFlags.Public | BindingFlags.Static);

            var nextIf = ilGenerator.DefineLabel();
            foreach (var knownType in knownTypes)
            {
                ilGenerator.MarkLabel(nextIf);
                nextIf = ilGenerator.DefineLabel();

                ilGenerator.Emit(OpCodes.Ldtoken, serviceTypeParameter);
                ilGenerator.Emit(OpCodes.Call, getType);
                ilGenerator.Emit(OpCodes.Ldtoken, knownType.Key);
                ilGenerator.Emit(OpCodes.Call, getType);
                ilGenerator.Emit(OpCodes.Call, equalityOperator);
                ilGenerator.Emit(OpCodes.Brfalse_S, nextIf);

                knownType.Value(ilGenerator);

                ilGenerator.Emit(OpCodes.Unbox_Any, serviceTypeParameter);
                ilGenerator.Emit(OpCodes.Ret);
            }

            ilGenerator.MarkLabel(nextIf);

            ilGenerator.Emit(OpCodes.Ldstr, "There is no provider for your service");
            var defaultConstructor = typeof(NotImplementedException).GetConstructor(new[]
            {
                typeof(string)
            });
            ilGenerator.Emit(OpCodes.Newobj, defaultConstructor);
            ilGenerator.Emit(OpCodes.Throw);

            flatContainer.DefineMethodOverride(methodBuilder, resolveMethod);

            methodBuilder.SetImplementationFlags(MethodImplAttributes.AggressiveInlining);

            return methodBuilder;
        }

        private TypeBuilder GenerateFlatContainer()
        {
            var typeBuilder = dynamicModule.DefineType(
                "Container",
                TypeAttributes.Public
                | TypeAttributes.Class
                | TypeAttributes.AutoClass
                | TypeAttributes.AnsiClass
                | TypeAttributes.AutoLayout
                | TypeAttributes.Sealed,
                typeof(object),
                new[]
                {
                    typeof(IContainer)
                });

            DefineDefaultConstructor(typeBuilder);

            return typeBuilder;
        }

        private static void DefineDefaultConstructor(TypeBuilder typeBuilder)
        {
            var constructor = typeof(object).GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                .First();
            var parameters = constructor.GetParameters();
            var constructorParameters = parameters.Select(info => info.ParameterType).ToArray();

            var constructorBuilder = typeBuilder.DefineConstructor(
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                CallingConventions.Standard,
                constructorParameters);

            var ilGenerator = constructorBuilder.GetILGenerator();

            ilGenerator.Emit(OpCodes.Ldarg_0);

            for (var i = 1; i < parameters.Length; i++)
            {
                ilGenerator.Emit(OpCodes.Ldarg, i);
            }

            ilGenerator.Emit(OpCodes.Call, constructor);
            ilGenerator.Emit(OpCodes.Ret);
        }
    }
}
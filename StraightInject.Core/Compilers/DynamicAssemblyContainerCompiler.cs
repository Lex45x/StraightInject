﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Lokad.ILPack;
using StraightInject.Core.Debugging;
using StraightInject.Core.ServiceConstructors;
using StraightInject.Services;

namespace StraightInject.Core.Compilers
{
    /// <summary>
    /// Gives a skeleton of container without resolve method body implementation
    /// </summary>
    internal abstract class DynamicAssemblyContainerCompilerBase : IContainerCompiler
    {
        private readonly Dictionary<Type, IServiceCompiler> dependencyConstructors;
        protected FieldInfo StateField;
        protected IContainerInitialState InitialState;
        private readonly ModuleBuilder dynamicModule;
        private readonly AssemblyBuilder assembly;

        protected DynamicAssemblyContainerCompilerBase(Dictionary<Type, IServiceCompiler> dependencyConstructors)
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

        public IContainer CompileDependencies(Dictionary<Type, IService> dependencies)
        {
            InitialState = new ContainerInitialState();

            var flatContainer = GenerateFlatContainer();

            var knownTypes = GenerateIlAppenders(flatContainer, dependencies);
            var resolveMethod = AppendResolveMethod(flatContainer, knownTypes);

            var type = flatContainer.CreateTypeInfo();

            if (DebugMode.Enabled())
            {
                var assemblyGenerator = new AssemblyGenerator();
                var combine = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                Console.WriteLine($"StraightInject generated assembly :: {combine}");
                assemblyGenerator.GenerateAssembly(assembly, combine);
            }

            return Activator.CreateInstance(type, InitialState) as IContainer;
        }

        private Dictionary<Type, Action<ILGenerator>> GenerateIlAppenders(Type flatContainer,
            Dictionary<Type, IService> dependencies)
        {
            var knownTypes = new Dictionary<Type, Action<ILGenerator>>();

            foreach (var (key, value) in dependencies)
            {
                var action = dependencyConstructors[value.GetType()]
                    .Compile(flatContainer, value, knownTypes, dependencies, InitialState, StateField);
                knownTypes.Add(key, action);
            }

            return knownTypes;
        }

        protected abstract void AppendResolveMethodBody(ILGenerator body,
            Type genericParameter,
            Dictionary<Type, Action<ILGenerator>> knownTypes);

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
            var serviceTypeParameter = genericParameters.First();
            var ilGenerator = methodBuilder.GetILGenerator();

            AppendResolveMethodBody(ilGenerator, serviceTypeParameter, knownTypes);

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

        private void DefineDefaultConstructor(TypeBuilder typeBuilder)
        {
            var constructor = typeof(object).GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                .First();

            StateField = typeBuilder.DefineField("_state", typeof(IContainerInitialState), FieldAttributes.Private);

            var constructorBuilder = typeBuilder.DefineConstructor(
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                CallingConventions.Standard,
                new[] {typeof(IContainerInitialState)});

            var ilGenerator = constructorBuilder.GetILGenerator();

            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Stfld, StateField);

            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Call, constructor);
            ilGenerator.Emit(OpCodes.Ret);
        }

        private class ContainerInitialState : IContainerInitialState
        {
            public Dictionary<Type, object> ComponentInstances { get; } = new Dictionary<Type, object>();
        }
    }
}
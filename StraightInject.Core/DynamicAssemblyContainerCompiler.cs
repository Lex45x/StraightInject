using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
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

            if (Debugger.IsAttached)
            {
                var assemblyGenerator = new AssemblyGenerator();
                var combine = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                
                Debug.Print($"StraightInject generated assembly :: {combine}");
                assemblyGenerator.GenerateAssembly(assembly, combine);
            }


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

        protected virtual void AppendResolveMethodBody(ILGenerator body,
            Type genericParameter,
            Dictionary<Type, Action<ILGenerator>> knownTypes)
        {
            var getType = typeof(Type).GetMethod("GetTypeFromHandle", BindingFlags.Public | BindingFlags.Static);
            var equalityOperator = typeof(Type).GetMethod("op_Equality", BindingFlags.Public | BindingFlags.Static);

            var nextIf = body.DefineLabel();
            foreach (var knownType in knownTypes)
            {
                body.MarkLabel(nextIf);
                nextIf = body.DefineLabel();

                body.Emit(OpCodes.Ldtoken, genericParameter);
                body.Emit(OpCodes.Call, getType);
                body.Emit(OpCodes.Ldtoken, knownType.Key);
                body.Emit(OpCodes.Call, getType);
                body.Emit(OpCodes.Call, equalityOperator);
                body.Emit(OpCodes.Brfalse_S, nextIf);

                knownType.Value(body);

                body.Emit(OpCodes.Unbox_Any, genericParameter);
                body.Emit(OpCodes.Ret);
            }

            body.MarkLabel(nextIf);

            body.Emit(OpCodes.Ldstr, "There is no provider for your service");
            var defaultConstructor = typeof(NotImplementedException).GetConstructor(new[]
            {
                typeof(string)
            });
            body.Emit(OpCodes.Newobj, defaultConstructor);
            body.Emit(OpCodes.Throw);
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace StraightInject.Core
{
    internal class ContainerCompiler : IContainerCompiler
    {
        private readonly Dictionary<Type, IDependencyConstructor> dependencyConstructors;

        private readonly ModuleBuilder dynamicModule;
        private readonly AssemblyBuilder assembly;

        public ContainerCompiler(Dictionary<Type, IDependencyConstructor> dependencyConstructors)
        {
            this.dependencyConstructors = dependencyConstructors;
            var assemblyName =
                new AssemblyName($"{typeof(ContainerCompiler).FullName}_Assembly_{Guid.NewGuid().ToString()}");
            assembly = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);

            dynamicModule =
                assembly.DefineDynamicModule(
                    $"{typeof(ContainerCompiler).FullName}_Module_{Guid.NewGuid().ToString()}");
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

            foreach (var dependency in dependencies)
            {
                dependencyConstructors[dependency.Value.GetType()]
                    .Construct(dependency.Key, dependency.Value, knownTypes, dependencies, new Stack<Type>());
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
            var defaultConstructor = typeof(NotImplementedException).GetConstructor(new []{typeof(string)});
            ilGenerator.Emit(OpCodes.Newobj, defaultConstructor);
            ilGenerator.Emit(OpCodes.Throw);

            flatContainer.DefineMethodOverride(methodBuilder, resolveMethod);

            methodBuilder.SetImplementationFlags(MethodImplAttributes.AggressiveInlining);

            return methodBuilder;
        }

        private TypeBuilder GenerateFlatContainer()
        {
            var typeBuilder = dynamicModule.DefineType(
                $"Container_{Guid.NewGuid().ToString()}",
                TypeAttributes.Public
                | TypeAttributes.Class
                | TypeAttributes.AutoClass
                | TypeAttributes.AnsiClass
                | TypeAttributes.AutoLayout
                | TypeAttributes.Sealed,
                typeof(object),
                new[] {typeof(IContainer)});

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

            for (var i = 0; i < parameters.Length + 1; i++)
            {
                ilGenerator.Emit(OpCodes.Ldarg, i);
            }

            ilGenerator.Emit(OpCodes.Call, constructor);
            ilGenerator.Emit(OpCodes.Ret);
        }
    }
}
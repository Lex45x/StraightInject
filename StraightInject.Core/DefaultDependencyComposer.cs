using System;
using System.Collections.Generic;

namespace StraightInject.Core
{
    internal class DefaultDependencyComposer : IDependencyMapper
    {
        private readonly IContainerCompiler compiler;
        private readonly Dictionary<Type, IService> dependencies;

        public static DefaultDependencyComposer Initialize()
        {
            return new DefaultDependencyComposer(
                new DynamicAssemblyBinarySearchByHashCodeContainerCompiler(
                    new Dictionary<Type, IServiceConstructor>
                    {
                        [typeof(TypedService)] = new TypedServiceConstructor()
                    }));
        }

        internal DefaultDependencyComposer(IContainerCompiler compiler)
        {
            this.compiler = compiler;
            dependencies = new Dictionary<Type, IService>();
        }

        public IComponentComposer<TComponent> FromType<TComponent>()
        {
            var wrapper = new TypedComponentComposer<TComponent>(dependencies);

            return wrapper;
        }

        public IComponentComposer FromType(Type implementationType)
        {
            return new TypeComponentComposer(implementationType, dependencies);
        }

        public IComponentComposer<TComponent> FromInstance<TComponent>(TComponent instance)
        {
            return new InstanceComponentComposer<TComponent>(dependencies, instance);
        }

        public IContainer Compile()
        {
            return compiler.CompileDependencies(dependencies);
        }
    }
}
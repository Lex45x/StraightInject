using System;
using System.Collections.Generic;
using StraightInject.Core.ComponentComposers;
using StraightInject.Core.ServiceConstructors;
using StraightInject.Core.Services;
using StraightInject.Services;

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
                    new Dictionary<Type, IServiceCompiler>
                    {
                        [typeof(TypedService)] = new TypedServiceCompiler()
                    }));
        }

        internal DefaultDependencyComposer(IContainerCompiler compiler)
        {
            this.compiler = compiler;
            dependencies = new Dictionary<Type, IService>();
        }

        public IComponentComposer<IConstructableService, TComponent> FromType<TComponent>()
        {
            var wrapper = new TypedComponentComposer<TComponent>(dependencies);

            return wrapper;
        }

        public IComponentComposer<IConstructableService> FromType(Type implementationType)
        {
            return new TypedComponentComposer(implementationType, dependencies);
        }

        public IComponentComposer<IService,TComponent> FromInstance<TComponent>(TComponent instance)
        {
            return new InstanceComponentComposer<TComponent>(dependencies, instance);
        }

        public IContainer Compile()
        {
            return compiler.CompileDependencies(dependencies);
        }
    }

    internal interface IContainerInitialState
    {

    }
}
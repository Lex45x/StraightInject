using System;
using System.Collections.Generic;
using StraightInject.Core.Compilers;
using StraightInject.Core.ComponentComposers;
using StraightInject.Core.ServiceConstructors;
using StraightInject.Core.Services;
using StraightInject.Services;

namespace StraightInject.Core
{
    /// <summary>
    /// Default composer of the whole dependency hierarchy
    /// </summary>
    internal class DefaultDependencyComposer : IDependencyMapper
    {
        private readonly IContainerCompiler compiler;
        private readonly Dictionary<Type, IService> dependencies;

        /// <summary>
        /// Create an instance with default settings
        /// </summary>
        /// <returns></returns>
        public static DefaultDependencyComposer Initialize()
        {
            return new DefaultDependencyComposer(
                new DynamicAssemblyJumpTableOfTypeHandleContainerCompiler(
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

        public IComponentComposer<IService, TComponent> FromInstance<TComponent>(TComponent instance)
        {
            return new InstanceComponentComposer<TComponent>(dependencies, instance);
        }

        public IContainer Compile()
        {
            return compiler.CompileDependencies(dependencies);
        }
    }
}
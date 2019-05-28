using System;
using System.Collections.Generic;

namespace StraightInject.Core
{
    internal class DefaultDependencyMapper : IDependencyMapper
    {
        private readonly IContainerCompiler compiler;
        private readonly Dictionary<Type, IDependency> dependencies;

        public static DefaultDependencyMapper Initialize()
        {
            return new DefaultDependencyMapper(
                new ContainerCompiler(
                    new Dictionary<Type, IDependencyConstructor>
                    {
                        [typeof(TypeDependency)] = new TypeDependencyConstructor()
                    }));
        }

        internal DefaultDependencyMapper(IContainerCompiler compiler)
        {
            this.compiler = compiler;
            dependencies = new Dictionary<Type, IDependency>();
        }

        public IDependency<T> MapType<T>()
        {
            var dependency = new TypeDependency(typeof(T), dependencies);

            var wrapper = new DependencyWrapper<T>(dependency);

            return wrapper;
        }

        public IContainer Compile()
        {
            return compiler.CompileDependencies(dependencies);
        }

        internal class DependencyWrapper<T> : IDependency<T>
        {
            private readonly IDependency implementation;

            public DependencyWrapper(IDependency implementation)
            {
                this.implementation = implementation;
            }

            public void SetServiceType<TService>()
            {
                implementation.SetServiceType<TService>();
            }
        }
    }
}
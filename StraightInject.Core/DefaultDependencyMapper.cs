using System;
using System.Collections.Generic;

namespace StraightInject.Core
{
    internal class DefaultDependencyComposer : IDependencyMapper
    {
        private readonly IContainerCompiler compiler;
        private readonly Dictionary<Type, IDependency> dependencies;

        public static DefaultDependencyComposer Initialize()
        {
            return new DefaultDependencyComposer(
                new DynamicAssemblyBinarySearchByHashCodeContainerCompiler(
                    new Dictionary<Type, IDependencyConstructor>
                    {
                        [typeof(TypeDependency)] = new TypeDependencyConstructor()
                    }));
        }

        internal DefaultDependencyComposer(IContainerCompiler compiler)
        {
            this.compiler = compiler;
            dependencies = new Dictionary<Type, IDependency>();
        }

        public IComponentComposer<TComponent> FromType<TComponent>()
        {
            var wrapper = new DefaultComposer<TComponent>(dependencies);

            return wrapper;
        }

        public IComponentComposer FromType(Type implementationType)
        {
            return new DefaultComposer(implementationType, dependencies);
        }

        public IContainer Compile()
        {
            return compiler.CompileDependencies(dependencies);
        }

        internal class DefaultComposer<T> : IComponentComposer<T>
        {
            private readonly Dictionary<Type, IDependency> dependencies;

            public DefaultComposer(Dictionary<Type, IDependency> dependencies)
            {
                this.dependencies = dependencies;
            }

            public void ToService<TService>()
            {
                dependencies.Add(typeof(TService), new TypeDependency(typeof(T)));
            }

            public void ToService(Type serviceType)
            {
                dependencies.Add(serviceType, new TypeDependency(typeof(T)));
            }
        }

        internal class DefaultComposer : IComponentComposer
        {
            private readonly Type componentType;
            private readonly Dictionary<Type, IDependency> dependencies;

            public DefaultComposer(Type componentType, Dictionary<Type, IDependency> dependencies)
            {
                this.componentType = componentType;
                this.dependencies = dependencies;
            }

            public void ToService<TService>()
            {
                dependencies.Add(typeof(TService), new TypeDependency(componentType));
            }

            public void ToService(Type serviceType)
            {
                dependencies.Add(serviceType, new TypeDependency(componentType));
            }
        }
    }
}
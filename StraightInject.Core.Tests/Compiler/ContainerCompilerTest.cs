using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using StraightInject.Core.Tests.Services;

namespace StraightInject.Core.Tests.Compiler
{
    [TestFixture]
    public class ContainerCompilerTest
    {
        [Test]
        public void EmptyContainerCompilationTest()
        {
            var compiler = new DynamicAssemblyBinarySearchByHashCodeContainerCompiler(new Dictionary<Type, IDependencyConstructor>());

            var container = compiler.CompileDependencies(new Dictionary<Type, IDependency>());

            Assert.IsNotNull(container);
            Assert.Throws<NotImplementedException>(() => container.Resolve<object>());
        }


        [Test]
        public void ContainerCompilationTest()
        {
            var compiler = new DynamicAssemblyBinarySearchByTypeHandleContainerCompiler(new Dictionary<Type, IDependencyConstructor>
            {
                [typeof(TypeDependency)] = new TypeDependencyConstructor()
            });

            var dependencies = new Dictionary<Type, IDependency>();

            dependencies.Add(typeof(IDependentService), new TypeDependency(typeof(DependentService), dependencies));
            dependencies.Add(typeof(IDependencyService), new TypeDependency(typeof(DependencyService), dependencies));
            dependencies.Add(typeof(IPlainService), new TypeDependency(typeof(PlainService), dependencies));
            dependencies.Add(typeof(PlainService), new TypeDependency(typeof(PlainService), dependencies));
            dependencies.Add(typeof(DependentService), new TypeDependency(typeof(DependentService), dependencies));
            dependencies.Add(typeof(DependencyService), new TypeDependency(typeof(DependencyService), dependencies));

            var container = compiler.CompileDependencies(dependencies);

            Assert.IsNotNull(container);
            Assert.Throws<NotImplementedException>(() => container.Resolve<object>());

            Assert.DoesNotThrow(() => container.Resolve<IDependentService>());
            Assert.DoesNotThrow(() => container.Resolve<IDependencyService>());
            Assert.DoesNotThrow(() => container.Resolve<IPlainService>());
            Assert.DoesNotThrow(() => container.Resolve<PlainService>());
            Assert.DoesNotThrow(() => container.Resolve<DependentService>());
            Assert.DoesNotThrow(() => container.Resolve<DependencyService>());
        }
    }
}
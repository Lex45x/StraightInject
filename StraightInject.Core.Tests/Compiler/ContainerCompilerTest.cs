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
            var compiler = new DynamicAssemblyContainerCompiler(new Dictionary<Type, IDependencyConstructor>());

            var container = compiler.CompileDependencies(new Dictionary<Type, IDependency>());

            Assert.IsNotNull(container);
            Assert.Throws<NotImplementedException>(() => container.Resolve<object>());
        }


        [Test]
        public void ContainerCompilationTest()
        {
            var compiler = new DynamicAssemblyContainerCompiler(new Dictionary<Type, IDependencyConstructor>
            {
                [typeof(TypeDependency)] = new TypeDependencyConstructor()
            });

            var dependencies = new Dictionary<Type, IDependency>();

            dependencies.Add(typeof(IDependentService), new TypeDependency(typeof(DependentService), dependencies));
            dependencies.Add(typeof(IDependencyService), new TypeDependency(typeof(DependencyService), dependencies));
            dependencies.Add(typeof(IPlainService), new TypeDependency(typeof(PlainService), dependencies));

            var container = compiler.CompileDependencies(dependencies);

            Assert.IsNotNull(container);
            Assert.Throws<NotImplementedException>(() => container.Resolve<object>());

            Assert.DoesNotThrow(() => container.Resolve<IDependentService>());
            Assert.DoesNotThrow(() => container.Resolve<IDependencyService>());
            Assert.DoesNotThrow(() => container.Resolve<IPlainService>());
        }
    }
}
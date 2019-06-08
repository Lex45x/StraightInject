using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using StraightInject.Core.Tests.Services;
using StraightInject.Core.Tests.Services.EmptyServices;

namespace StraightInject.Core.Tests.Compiler
{
    [TestFixture]
    public class ContainerCompilerTest
    {
        [Test]
        public void EmptyContainerCompilationTest()
        {
            var compiler = new DynamicAssemblyTypeHandleJumpTableContainerCompiler(new Dictionary<Type, IDependencyConstructor>());

            var container = compiler.CompileDependencies(new Dictionary<Type, IDependency>());

            Assert.IsNotNull(container);
            Assert.Throws<NotImplementedException>(() => container.Resolve<object>());
        }


        [Test]
        public void ContainerCompilationTest()
        {
            Environment.SetEnvironmentVariable("STRAIGHT_INJECT_ENABLE_DIAGNOSTIC", "true");

            var compiler = new DynamicAssemblyTypeHandleJumpTableContainerCompiler(new Dictionary<Type, IDependencyConstructor>
            {
                [typeof(TypeDependency)] = new TypeDependencyConstructor()
            });

            var dependencies = new Dictionary<Type, IDependency>();

            foreach (var service in Assembly.GetExecutingAssembly().ExportedTypes
                .Where(type => type.GetInterfaces().Contains(typeof(IEmptyService))))
            {
                dependencies.Add(service, new TypeDependency(service));
            }

            dependencies.Add(typeof(IDependentService), new TypeDependency(typeof(DependentService)));
            dependencies.Add(typeof(IDependencyService), new TypeDependency(typeof(DependencyService)));
            dependencies.Add(typeof(IPlainService), new TypeDependency(typeof(PlainService)));
            dependencies.Add(typeof(PlainService), new TypeDependency(typeof(PlainService)));
            dependencies.Add(typeof(DependentService), new TypeDependency(typeof(DependentService)));
            dependencies.Add(typeof(DependencyService), new TypeDependency(typeof(DependencyService)));

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
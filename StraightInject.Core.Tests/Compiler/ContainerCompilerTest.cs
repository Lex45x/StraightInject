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
            var compiler = new DynamicAssemblyTypeHandleJumpTableContainerCompiler(new Dictionary<Type, IServiceConstructor>());

            var container = compiler.CompileDependencies(new Dictionary<Type, IService>());

            Assert.IsNotNull(container);
            Assert.Throws<NotImplementedException>(() => container.Resolve<object>());
        }


        [Test]
        public void ContainerCompilationTest()
        {
            Environment.SetEnvironmentVariable("STRAIGHT_INJECT_ENABLE_DIAGNOSTIC", "true");

            var compiler = new DynamicAssemblyTypeHandleJumpTableContainerCompiler(new Dictionary<Type, IServiceConstructor>
            {
                [typeof(TypedService)] = new TypedServiceConstructor()
            });

            var dependencies = new Dictionary<Type, IService>();

            foreach (var service in Assembly.GetExecutingAssembly().ExportedTypes
                .Where(type => type.GetInterfaces().Contains(typeof(IEmptyService))))
            {
                dependencies.Add(service, new TypedService(service));
            }

            dependencies.Add(typeof(IDependentService), new TypedService(typeof(DependentService)));
            dependencies.Add(typeof(IDependencyService), new TypedService(typeof(DependencyService)));
            dependencies.Add(typeof(IPlainService), new TypedService(typeof(PlainService)));
            dependencies.Add(typeof(PlainService), new TypedService(typeof(PlainService)));
            dependencies.Add(typeof(DependentService), new TypedService(typeof(DependentService)));
            dependencies.Add(typeof(DependencyService), new TypedService(typeof(DependencyService)));

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
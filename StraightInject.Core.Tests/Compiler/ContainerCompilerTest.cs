using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using StraightInject.Core.Compilers;
using StraightInject.Core.ConstructorResolver;
using StraightInject.Core.ServiceConstructors;
using StraightInject.Core.Services;
using StraightInject.Core.Tests.Services;
using StraightInject.Core.Tests.Services.EmptyServices;
using StraightInject.Core.Tests.Services.MVC.Configuration;
using StraightInject.Core.Tests.Services.MVC.DataAccess;
using StraightInject.Services;

namespace StraightInject.Core.Tests.Compiler
{
    [TestFixture]
    public class ContainerCompilerTest
    {
        [Test]
        public void EmptyContainerCompilationTest()
        {
            var compiler =
                new DynamicAssemblyJumpTableOfTypeHandleContainerCompiler(new Dictionary<Type, IServiceCompiler>());

            var container = compiler.CompileDependencies(new Dictionary<Type, IService>());

            Assert.IsNotNull(container);
            Assert.Throws<InvalidOperationException>(() => container.Resolve<object>());
        }


        [Test]
        public void ContainerCompilationTest()
        {
            var compiler = new DynamicAssemblyJumpTableOfTypeHandleContainerCompiler(
                new Dictionary<Type, IServiceCompiler>
                {
                    [typeof(TypedService)] = new TypedServiceCompiler()
                });

            var dependencies = new Dictionary<Type, IService>();

            foreach (var service in Assembly.GetExecutingAssembly().ExportedTypes
                .Where(type => type.GetInterfaces().Contains(typeof(IEmptyService))))
            {
                dependencies.Add(service, new TypedService(service, new EagerConstructorResolver(), service));
            }


            dependencies.Add(typeof(IUserRepository),
                new TypedService(typeof(UnitOfWork), new EagerConstructorResolver(), typeof(IUserRepository)));
            dependencies.Add(typeof(IDatabaseConfiguration),
                new TypedService(typeof(DatabaseConfiguration), new EagerConstructorResolver(),
                    typeof(IDatabaseConfiguration)));
            dependencies.Add(typeof(ICacheConfiguration),
                new TypedService(typeof(CacheConfiguration), new EagerConstructorResolver(),
                    typeof(ICacheConfiguration)));
            dependencies.Add(typeof(CacheConfiguration),
                new TypedService(typeof(CacheConfiguration), new EagerConstructorResolver(),
                    typeof(CacheConfiguration)));
            dependencies.Add(typeof(UnitOfWork),
                new TypedService(typeof(UnitOfWork), new EagerConstructorResolver(), typeof(UnitOfWork)));
            dependencies.Add(typeof(DatabaseConfiguration),
                new TypedService(typeof(DatabaseConfiguration), new EagerConstructorResolver(),
                    typeof(DatabaseConfiguration)));

            var container = compiler.CompileDependencies(dependencies);

            Assert.IsNotNull(container);
            Assert.Throws<InvalidOperationException>(() => container.Resolve<object>());

            Assert.DoesNotThrow(() => container.Resolve<IUserRepository>());
            Assert.DoesNotThrow(() => container.Resolve<IDatabaseConfiguration>());
            Assert.DoesNotThrow(() => container.Resolve<ICacheConfiguration>());
            Assert.DoesNotThrow(() => container.Resolve<CacheConfiguration>());
            Assert.DoesNotThrow(() => container.Resolve<UnitOfWork>());
            Assert.DoesNotThrow(() => container.Resolve<DatabaseConfiguration>());
        }
    }
}
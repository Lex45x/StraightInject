using System;
using System.Collections.Generic;
using NUnit.Framework;
using StraightInject.Core.Compilers;
using StraightInject.Core.ConstructorResolver;
using StraightInject.Core.ServiceConstructors;
using StraightInject.Core.Services;
using StraightInject.Core.Tests.Services;
using StraightInject.Core.Tests.Services.MVC.Configuration;
using StraightInject.Services;

namespace StraightInject.Core.Tests.Compiler
{
    [TestFixture]
    public class SingletonServiceCompilerTest
    {
        [Test]
        public void CompilationTest()
        {
            var compiler = new SingletonServiceCompiler();
            var stubCompiler = new DynamicAssemblyJumpTableOfTypeHandleContainerCompiler(
                new Dictionary<Type, IServiceCompiler>
                {
                    [typeof(SingletonService)] = compiler
                });

            var interfaceService = new SingletonService(typeof(CacheConfiguration), new EagerConstructorResolver(),
                typeof(ICacheConfiguration));
            var classService = new SingletonService(typeof(CacheConfiguration), new EagerConstructorResolver(),
                typeof(CacheConfiguration));

            var dependencies = new Dictionary<Type, IService>
            {
                [typeof(ICacheConfiguration)] = interfaceService,
                [typeof(CacheConfiguration)] = classService
            };

            var container = stubCompiler.CompileDependencies(dependencies);

            var plainServiceInterface = container.Resolve<ICacheConfiguration>();
            var plainService = container.Resolve<CacheConfiguration>();

            Assert.IsNotNull(plainService);
            Assert.IsNotNull(plainServiceInterface);
            Assert.AreSame(plainService, plainServiceInterface);
        }
    }
}
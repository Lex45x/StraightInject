using System;
using System.Collections.Generic;
using NUnit.Framework;
using StraightInject.Core.Compilers;
using StraightInject.Core.ConstructorResolver;
using StraightInject.Core.ServiceConstructors;
using StraightInject.Core.Services;
using StraightInject.Core.Tests.Services;
using StraightInject.Services;

namespace StraightInject.Core.Tests.Compiler
{
    [TestFixture]
    public class SingletonServiceCompilerTest
    {
        [Test]
        public void CompilationTest()
        {
            Environment.SetEnvironmentVariable("STRAIGHT_INJECT_ENABLE_DIAGNOSTIC", "true");

            var compiler = new SingletonServiceCompiler();
            var stubCompiler = new DynamicAssemblyJumpTableOfTypeHandleContainerCompiler(
                new Dictionary<Type, IServiceCompiler>
                {
                    [typeof(SingletonService)] = compiler
                });

            var interfaceService = new SingletonService(typeof(PlainService), new EagerConstructorResolver(),
                typeof(IPlainService));
            var classService = new SingletonService(typeof(PlainService), new EagerConstructorResolver(),
                typeof(PlainService));

            var dependencies = new Dictionary<Type, IService>
            {
                [typeof(IPlainService)] = interfaceService,
                [typeof(PlainService)] = classService
            };

            var container = stubCompiler.CompileDependencies(dependencies);

            var plainServiceInterface = container.Resolve<IPlainService>();
            var plainService = container.Resolve<PlainService>();

            Assert.IsNotNull(plainService);
            Assert.IsNotNull(plainServiceInterface);
            Assert.AreSame(plainService, plainServiceInterface);
        }
    }
}
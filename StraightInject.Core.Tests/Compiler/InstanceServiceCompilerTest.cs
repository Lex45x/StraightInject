﻿using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using NUnit.Framework;
using StraightInject.Core.Compilers;
using StraightInject.Core.ServiceConstructors;
using StraightInject.Core.Services;
using StraightInject.Core.Tests.Services;
using StraightInject.Core.Tests.Services.MVC.Configuration;
using StraightInject.Services;

namespace StraightInject.Core.Tests.Compiler
{
    [TestFixture]
    public class InstanceServiceCompilerTest
    {
        [Test]
        public void CompilationTest()
        {
            var compiler = new InstanceServiceCompiler();
            var stubCompiler = new DynamicAssemblyJumpTableOfTypeHandleContainerCompiler(new Dictionary<Type, IServiceCompiler>
            {
                [typeof(InstanceService)] = compiler
            });

            var instance = new CacheConfiguration();
            var instanceService = new InstanceService(instance, typeof(ICacheConfiguration));

            var dependencies = new Dictionary<Type, IService>
            {
                [typeof(ICacheConfiguration)] = instanceService
            };

            var container = stubCompiler.CompileDependencies(dependencies);

            var plainService = container.Resolve<ICacheConfiguration>();

            Assert.IsNotNull(plainService);
            Assert.AreSame(plainService, instance);
        }
    }
}
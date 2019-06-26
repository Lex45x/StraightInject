using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Moq;
using NUnit.Framework;
using StraightInject.Core.ConstructorResolver;
using StraightInject.Core.ServiceConstructors;
using StraightInject.Core.Services;
using StraightInject.Core.Tests.Services;
using StraightInject.Core.Tests.Services.MVC.Configuration;
using StraightInject.Core.Tests.Services.MVC.DataAccess;
using StraightInject.Services;

namespace StraightInject.Core.Tests.Compiler
{
    [TestFixture]
    public class TypedServiceCompilerTests : ServiceCompilerTestBase
    {
        [Test]
        public void InvalidDependencyTypeTest()
        {
            var constructor = new TypedServiceCompiler();
            var knownTypes = new Dictionary<Type, Action<ILGenerator>>();
            var dependencies = new Dictionary<Type, IService>();
            var serviceMock = Mock.Of<IService>(service => service.ServiceType == GetType());

            Assert.Throws<InvalidOperationException>(
                () => constructor.Compile(null, serviceMock, knownTypes, dependencies, null, null));
        }

        [Test]
        public void DependencyWithMissingReferenceTest()
        {
            var constructor = new TypedServiceCompiler();

            var typeDependency = new TypedService(typeof(UnitOfWork), new EagerConstructorResolver(),
                typeof(UnitOfWork));
            var dependencies = new Dictionary<Type, IService>
            {
                [typeof(UnitOfWork)] = typeDependency
            };

            Assert.Throws<InvalidOperationException>(() =>
                constructor.Compile(null, typeDependency, new Dictionary<Type, Action<ILGenerator>>(), dependencies,
                    null, null));
        }

        [Test]
        public void DependencyWithEagerConstructorTest()
        {
            var compiler = new TypedServiceCompiler();

            var typeDependency = new TypedService(typeof(UnitOfWork), new EagerConstructorResolver(),
                typeof(UnitOfWork));
            var plainServiceDependency = new TypedService(typeof(DatabaseConfiguration), new EagerConstructorResolver(),
                typeof(IDatabaseConfiguration));

            var dependencies = new Dictionary<Type, IService>
            {
                [typeof(UnitOfWork)] = typeDependency,
                [typeof(IDatabaseConfiguration)] = plainServiceDependency
            };

            var knownTypes = new Dictionary<Type, Action<ILGenerator>>();

            knownTypes.Add(typeof(IDatabaseConfiguration),
                compiler.Compile(null, plainServiceDependency, knownTypes, dependencies, null, null));

            var action = compiler.Compile(null, typeDependency, knownTypes, dependencies, null, null);

            Assert.IsNotNull(action);

            var instance = AssertIlValidity<UnitOfWork>(action);

            Assert.IsNotNull(instance.Configuration);
        }

        [Test]
        public void DependencyWithSpecifiedConstructorTest()
        {
            var compiler = new TypedServiceCompiler();

            var typeDependency = new TypedService(typeof(UnitOfWork),
                new ExpressionConstructorResolver<UnitOfWork>(() => new UnitOfWork()),
                typeof(UnitOfWork));

            var dependencies = new Dictionary<Type, IService>
            {
                [typeof(UnitOfWork)] = typeDependency
            };

            var knownTypes = new Dictionary<Type, Action<ILGenerator>>();

            var action = compiler.Compile(null, typeDependency, knownTypes, dependencies, null, null);

            Assert.IsNotNull(action);

            var instance = AssertIlValidity<UnitOfWork>(action);

            Assert.IsNull(instance.Configuration);
        }

        [Test]
        public void DependencyWithDependentServiceConstructorTest()
        {
            var constructor = new TypedServiceCompiler();

            var dependency = new TypedService(typeof(UnitOfWork), new EagerConstructorResolver(),
                typeof(UnitOfWork));

            var typedService = new TypedService(typeof(DatabaseConfiguration), new EagerConstructorResolver(),
                typeof(IDatabaseConfiguration));

            var dependencies = new Dictionary<Type, IService>
            {
                [typeof(IDatabaseConfiguration)] = typedService,
                [typeof(UnitOfWork)] = dependency
            };

            var knownTypes = new Dictionary<Type, Action<ILGenerator>>();

            var construct = constructor.Compile(null, typedService, knownTypes, dependencies, null, null);
            knownTypes.Add(typedService.ServiceType, construct);

            var action = constructor.Compile(null, dependency,
                knownTypes,
                dependencies, null, null);

            Assert.IsNotNull(action);
            AssertIlValidity(action, dependency.OriginalType);
        }
    }
}
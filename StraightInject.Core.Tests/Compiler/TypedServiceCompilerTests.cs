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
using StraightInject.Services;

namespace StraightInject.Core.Tests.Compiler
{
    [TestFixture]
    public class TypedServiceCompilerTests: ServiceCompilerTestBase
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

            var typeDependency = new TypedService(typeof(DependentService), new EagerConstructorResolver(),
                typeof(DependentService));
            var dependencies = new Dictionary<Type, IService>
            {
                [typeof(DependentService)] = typeDependency
            };

            Assert.Throws<InvalidOperationException>(() =>
                constructor.Compile(null, typeDependency, new Dictionary<Type, Action<ILGenerator>>(), dependencies, null, null));
        }

        [Test]
        public void DependencyWithEagerConstructorTest()
        {
            var compiler = new TypedServiceCompiler();

            var typeDependency = new TypedService(typeof(MultiConstructorService), new EagerConstructorResolver(),
                typeof(MultiConstructorService));
            var plainServiceDependency = new TypedService(typeof(PlainService), new EagerConstructorResolver(),
                typeof(IPlainService));

            var dependencies = new Dictionary<Type, IService>
            {
                [typeof(MultiConstructorService)] = typeDependency,
                [typeof(IPlainService)] = plainServiceDependency
            };

            var knownTypes = new Dictionary<Type, Action<ILGenerator>>();

            knownTypes.Add(typeof(IPlainService),
                compiler.Compile(null, plainServiceDependency, knownTypes, dependencies, null, null));

            var action = compiler.Compile(null, typeDependency, knownTypes, dependencies, null, null);

            Assert.IsNotNull(action);

            var instance = AssertIlValidity<MultiConstructorService>(action);

            Assert.IsNotNull(instance.Service);
        }

        [Test]
        public void DependencyWithSpecifiedConstructorTest()
        {
            var compiler = new TypedServiceCompiler();

            var typeDependency = new TypedService(typeof(MultiConstructorService),
                new ExpressionConstructorResolver<MultiConstructorService>(() => new MultiConstructorService()),
                typeof(MultiConstructorService));

            var dependencies = new Dictionary<Type, IService>
            {
                [typeof(MultiConstructorService)] = typeDependency
            };

            var knownTypes = new Dictionary<Type, Action<ILGenerator>>();

            var action = compiler.Compile(null, typeDependency, knownTypes, dependencies, null, null);

            Assert.IsNotNull(action);

            var instance = AssertIlValidity<MultiConstructorService>(action);

            Assert.IsNull(instance.Service);
        }

        [Test]
        public void DependencyWithDependentServiceConstructorTest()
        {
            var constructor = new TypedServiceCompiler();

            var dependency = new TypedService(typeof(DependentService), new EagerConstructorResolver(),
                typeof(DependentService));

            var typedService = new TypedService(typeof(DependencyService), new EagerConstructorResolver(),
                typeof(IDependencyService));

            var dependencies = new Dictionary<Type, IService>
            {
                [typeof(IDependencyService)] = typedService,
                [typeof(DependentService)] = dependency
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
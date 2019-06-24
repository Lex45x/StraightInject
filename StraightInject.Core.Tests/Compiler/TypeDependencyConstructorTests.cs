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
    public class TypeDependencyConstructorTests
    {
        [Test]
        public void InvalidDependencyTypeTest()
        {
            var constructor = new TypedServiceCompiler();
            var knownTypes = new Dictionary<Type, Action<ILGenerator>>();
            var dependencies = new Dictionary<Type, IService>();
            var serviceMock = Mock.Of<IService>(service => service.ServiceType == GetType());

            Assert.Throws<InvalidOperationException>(
                () => constructor.Construct(null, serviceMock, knownTypes, dependencies));
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
                constructor.Construct(null, typeDependency, new Dictionary<Type, Action<ILGenerator>>(), dependencies));
        }

        [Test]
        public void DependencyWithDefaultConstructorTest()
        {
            var constructor = new TypedServiceCompiler();

            var typeDependency = new TypedService(GetType(), new EagerConstructorResolver(), GetType());
            var dependencies = new Dictionary<Type, IService>
            {
                [GetType()] = typeDependency
            };

            var knownTypes = new Dictionary<Type, Action<ILGenerator>>();

            var action = constructor.Construct(null, typeDependency, knownTypes, dependencies);

            Assert.IsNotNull(action);

            AssertIlValidity(action, typeDependency.OriginalType);
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

            var construct = constructor.Construct(null, typedService, knownTypes, dependencies);
            knownTypes.Add(typedService.ServiceType, construct);

            var action = constructor.Construct(null, dependency,
                knownTypes,
                dependencies);

            Assert.IsNotNull(action);
            AssertIlValidity(action, dependency.OriginalType);
        }

        private void AssertIlValidity(Action<ILGenerator> action, Type expectedInstanceType)
        {
            var dynamicMethod = new DynamicMethod(Guid.NewGuid().ToString(), typeof(object), Type.EmptyTypes);
            var ilGenerator = dynamicMethod.GetILGenerator();

            action(ilGenerator);

            ilGenerator.Emit(OpCodes.Ret);

            var @delegate = dynamicMethod.CreateDelegate(typeof(Func<object>)) as Func<object>;

            Assert.IsNotNull(@delegate);

            var instance = @delegate();

            Assert.IsNotNull(instance);
            Assert.AreEqual(instance.GetType(), expectedInstanceType);
        }
    }
}
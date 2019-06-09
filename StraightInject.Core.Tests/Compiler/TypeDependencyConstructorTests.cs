using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Moq;
using NUnit.Framework;
using StraightInject.Core.Tests.Services;

namespace StraightInject.Core.Tests.Compiler
{
    [TestFixture]
    public class TypeDependencyConstructorTests
    {
        [Test]
        public void InvalidDependencyTypeTest()
        {
            var constructor = new TypedServiceConstructor();

            Assert.Throws<InvalidOperationException>(() =>
                constructor.Construct(
                    GetType(),
                    Mock.Of<IService>(),
                    new Dictionary<Type, Action<ILGenerator>>(),
                    new Dictionary<Type, IService>(),
                    new Stack<Type>()));
        }

        [Test]
        public void RecursiveDependencyTest()
        {
            var constructor = new TypedServiceConstructor();

            var typeDependency = new TypedService(GetType());
            var dependencies = new Dictionary<Type, IService>
            {
                [GetType()] = typeDependency
            };

            Assert.Throws<InvalidOperationException>(() =>
                constructor.Construct(
                    typeDependency.OriginalType,
                    typeDependency,
                    new Dictionary<Type, Action<ILGenerator>>(),
                    dependencies,
                    new Stack<Type>(Enumerable.Repeat(GetType(), 1))));
        }

        [Test]
        public void DependencyWithMissingReferenceTest()
        {
            var constructor = new TypedServiceConstructor();

            var typeDependency = new TypedService(typeof(DependentService));
            var dependencies = new Dictionary<Type, IService>
            {
                [typeof(DependentService)] = typeDependency
            };

            Assert.Throws<NotImplementedException>(() =>
                constructor.Construct(
                    typeDependency.OriginalType,
                    typeDependency,
                    new Dictionary<Type, Action<ILGenerator>>(),
                    dependencies,
                    new Stack<Type>()));
        }

        [Test]
        public void DependencyWithDefaultConstructorTest()
        {
            var constructor = new TypedServiceConstructor();

            var typeDependency = new TypedService(GetType());
            var dependencies = new Dictionary<Type, IService>
            {
                [GetType()] = typeDependency
            };

            var knownTypes = new Dictionary<Type, Action<ILGenerator>>();

            var action = constructor.Construct(
                typeDependency.OriginalType,
                typeDependency,
                knownTypes,
                new Dictionary<Type, IService>(),
                new Stack<Type>());
            
            Assert.IsNotEmpty(knownTypes);

            AssertIlValidity(action, typeDependency.OriginalType);
        }

        [Test]
        public void DependencyWithDependentServiceConstructorTest()
        {
            var constructor = new TypedServiceConstructor();

            var dependency = new TypedService(typeof(DependentService));
            var dependencies = new Dictionary<Type, IService>
            {
                [typeof(IDependencyService)] = new TypedService(typeof(DependencyService)),
                [typeof(DependentService)] = dependency
            };

            var knownTypes = new Dictionary<Type, Action<ILGenerator>>();

            var action = constructor.Construct(
                dependency.OriginalType,
                dependency,
                knownTypes,
                dependencies,
                new Stack<Type>());

            Assert.IsNotEmpty(knownTypes);
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
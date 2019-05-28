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
            var constructor = new TypeDependencyConstructor();

            Assert.Throws<InvalidOperationException>(() =>
                constructor.Construct(
                    GetType(),
                    Mock.Of<IDependency>(),
                    new Dictionary<Type, Action<ILGenerator>>(),
                    new Dictionary<Type, IDependency>(),
                    new Stack<Type>()));
        }

        [Test]
        public void RecursiveDependencyTest()
        {
            var constructor = new TypeDependencyConstructor();
            var dependency = new TypeDependency(GetType(), new Dictionary<Type, IDependency>());
            dependency.SetServiceType<TypeDependencyConstructorTests>();

            Assert.Throws<InvalidOperationException>(() =>
                constructor.Construct(
                    dependency.OriginalType,
                    dependency,
                    new Dictionary<Type, Action<ILGenerator>>(),
                    new Dictionary<Type, IDependency>(),
                    new Stack<Type>(Enumerable.Repeat(GetType(), 1))));
        }

        [Test]
        public void DependencyWithMissingReferenceTest()
        {
            var constructor = new TypeDependencyConstructor();
            var dependency = new TypeDependency(typeof(DependentService), new Dictionary<Type, IDependency>());

            Assert.Throws<NotImplementedException>(() =>
                constructor.Construct(
                    dependency.OriginalType,
                    dependency,
                    new Dictionary<Type, Action<ILGenerator>>(),
                    new Dictionary<Type, IDependency>(),
                    new Stack<Type>()));
        }

        [Test]
        public void DependencyWithDefaultConstructorTest()
        {
            var constructor = new TypeDependencyConstructor();
            var dependency = new TypeDependency(typeof(PlainService), new Dictionary<Type, IDependency>());
            var knownTypes = new Dictionary<Type, Action<ILGenerator>>();

            var action = constructor.Construct(
                dependency.OriginalType,
                dependency,
                knownTypes,
                new Dictionary<Type, IDependency>(),
                new Stack<Type>());
            
            Assert.IsNotEmpty(knownTypes);

            AssertIlValidity(action, dependency.OriginalType);
        }

        [Test]
        public void DependencyWithDependentServiceConstructorTest()
        {
            var constructor = new TypeDependencyConstructor();
            var dependencies = new Dictionary<Type, IDependency>
            {
                [typeof(IDependencyService)] = new TypeDependency(typeof(DependencyService), null)
            };

            var dependency = new TypeDependency(typeof(DependentService), dependencies);
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
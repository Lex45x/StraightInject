using System;
using System.Reflection.Emit;
using NUnit.Framework;

namespace StraightInject.Core.Tests.Compiler
{
    public abstract class ServiceCompilerTestBase
    {
        protected object AssertIlValidity(Action<ILGenerator> action, Type expectedInstanceType)
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

            return instance;
        }

        protected T AssertIlValidity<T>(Action<ILGenerator> action) where T : class
        {
            return AssertIlValidity(action, typeof(T)) as T;
        }
    }
}
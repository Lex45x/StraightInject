using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace StraightInject.Core
{
    internal class DynamicAssemblyBinarySearchByTypeHandleContainerCompiler : DynamicAssemblyContainerCompiler
    {
        public DynamicAssemblyBinarySearchByTypeHandleContainerCompiler(
            Dictionary<Type, IDependencyConstructor> dependencyConstructors) : base(dependencyConstructors)
        {
        }

        protected override void AppendResolveMethodBody(ILGenerator body, Type genericParameter,
            Dictionary<Type, Action<ILGenerator>> knownTypes)
        {
            var sortedSpan = new Span<Type>(knownTypes.Keys.OrderBy(type => type.TypeHandle.Value.ToInt64()).ToArray());

            var typeAddress = body.DeclareLocal(typeof(long));
            var typeHandle = body.DeclareLocal(typeof(RuntimeTypeHandle));
            var typePointer = body.DeclareLocal(typeof(IntPtr));

            var getRuntimeTypePointer = typeof(RuntimeTypeHandle).GetProperty("Value", BindingFlags.Public | BindingFlags.Instance).GetMethod;
            var toInt64 = typeof(IntPtr)
                .GetMethod("ToInt64", BindingFlags.Public | BindingFlags.Instance);
            
            var exceptionLabel = body.DefineLabel();
            
            body.Emit(OpCodes.Ldtoken, genericParameter);
            body.Emit(OpCodes.Stloc, typeHandle);
            body.Emit(OpCodes.Ldloca, typeHandle);
            body.Emit(OpCodes.Call, getRuntimeTypePointer);
            body.Emit(OpCodes.Stloc, typePointer);
            body.Emit(OpCodes.Ldloca, typePointer);
            body.Emit(OpCodes.Call, toInt64);
            body.Emit(OpCodes.Stloc, typeAddress);

            AppendBranch(sortedSpan, body, genericParameter, knownTypes, typeAddress, exceptionLabel);
            
            body.MarkLabel(exceptionLabel);
            body.Emit(OpCodes.Ldstr, "There is no provider for your service");
            var defaultConstructor = typeof(NotImplementedException).GetConstructor(new[]
            {
                typeof(string)
            });
            body.Emit(OpCodes.Newobj, defaultConstructor);
            body.Emit(OpCodes.Throw);
        }

        private void AppendBranch(Span<Type> branch,
            ILGenerator body,
            Type genericParameter,
            Dictionary<Type, Action<ILGenerator>> knownTypes,
            LocalBuilder typePointer,
            Label exceptionLabel)
        {
            if (branch.Length <= 0)
            {
                return;
            }

            if (branch.Length == 1)
            {
                body.Emit(OpCodes.Ldloc, typePointer);
                var type = branch[0];
                body.Emit(OpCodes.Ldc_I8, type.TypeHandle.Value.ToInt64());
                body.Emit(OpCodes.Bne_Un, exceptionLabel);

                knownTypes[type](body);
                body.Emit(OpCodes.Unbox_Any, genericParameter);
                body.Emit(OpCodes.Ret);

                return;
            }

            if (branch.Length == 2)
            {
                var nextComparison = body.DefineLabel();
                body.Emit(OpCodes.Ldloc, typePointer);
                var firstType = branch[0];
                body.Emit(OpCodes.Ldc_I8, firstType.TypeHandle.Value.ToInt64());
                body.Emit(OpCodes.Bne_Un, nextComparison);
                knownTypes[firstType](body);
                body.Emit(OpCodes.Unbox_Any, genericParameter);
                body.Emit(OpCodes.Ret);

                body.MarkLabel(nextComparison);
                body.Emit(OpCodes.Ldloc, typePointer);
                var secondType = branch[1];
                body.Emit(OpCodes.Ldc_I8, secondType.TypeHandle.Value.ToInt64());
                body.Emit(OpCodes.Bne_Un, exceptionLabel);
                knownTypes[secondType](body);
                body.Emit(OpCodes.Unbox_Any, genericParameter);
                body.Emit(OpCodes.Ret);

                return;
            }

            var halfLength = branch.Length / 2;
            var dividerCode = branch.Length % 2 == 0
                ? branch[halfLength].TypeHandle.Value.ToInt64() - 1
                : branch[halfLength].TypeHandle.Value.ToInt64();

            var @else = body.DefineLabel();

            body.Emit(OpCodes.Ldloc, typePointer);
            body.Emit(OpCodes.Ldc_I8, dividerCode);
            body.Emit(OpCodes.Bge, @else);

            var firstHalf = branch.Slice(0, halfLength);
            AppendBranch(firstHalf, body, genericParameter, knownTypes, typePointer, exceptionLabel);

            body.MarkLabel(@else);

            var secondHalf = branch.Slice(halfLength);
            AppendBranch(secondHalf, body, genericParameter, knownTypes, typePointer, exceptionLabel);
        }
    }
}
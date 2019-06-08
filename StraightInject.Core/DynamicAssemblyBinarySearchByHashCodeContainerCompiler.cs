using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace StraightInject.Core
{
    internal class DynamicAssemblyBinarySearchByHashCodeContainerCompiler : DynamicAssemblyContainerCompiler
    {
        public DynamicAssemblyBinarySearchByHashCodeContainerCompiler(
            Dictionary<Type, IDependencyConstructor> dependencyConstructors) : base(dependencyConstructors)
        {
        }

        protected override void AppendResolveMethodBody(ILGenerator body, Type genericParameter,
            Dictionary<Type, Action<ILGenerator>> knownTypes)
        {
            var sortedSpan = new Span<Type>(knownTypes.Keys.OrderBy(type => type.GetHashCode()).ToArray());

            var hashCode = body.DeclareLocal(typeof(int));

            var getType = typeof(Type).GetMethod("GetTypeFromHandle", BindingFlags.Public | BindingFlags.Static);
            var getHashCode = typeof(object).GetMethod("GetHashCode", BindingFlags.Public | BindingFlags.Instance);

            var exceptionLabel = body.DefineLabel();

            body.Emit(OpCodes.Ldtoken, genericParameter);
            body.Emit(OpCodes.Call, getType);
            body.Emit(OpCodes.Call, getHashCode);
            body.Emit(OpCodes.Stloc, hashCode);

            AppendBranch(sortedSpan, body, genericParameter, knownTypes, hashCode, exceptionLabel);

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
            LocalBuilder hashCode,
            Label exceptionLabel)
        {
            if (branch.Length <= 0)
            {
                return;
            }

            if (branch.Length == 1)
            {
                body.Emit(OpCodes.Ldloc, hashCode);
                var type = branch[0];
                body.Emit(OpCodes.Ldc_I4, type.GetHashCode());
                body.Emit(OpCodes.Bne_Un, exceptionLabel);

                knownTypes[type](body);
                body.Emit(OpCodes.Unbox_Any, genericParameter);
                body.Emit(OpCodes.Ret);

                return;
            }
            
            if (branch.Length == 2)
            {
                var nextComparison = body.DefineLabel();
                body.Emit(OpCodes.Ldloc, hashCode);
                var firstType = branch[0];
                body.Emit(OpCodes.Ldc_I4, firstType.GetHashCode());
                body.Emit(OpCodes.Bne_Un, nextComparison);
                knownTypes[firstType](body);
                body.Emit(OpCodes.Unbox_Any, genericParameter);
                body.Emit(OpCodes.Ret);

                body.MarkLabel(nextComparison);
                body.Emit(OpCodes.Ldloc, hashCode);
                var secondType = branch[1];
                body.Emit(OpCodes.Ldc_I4, secondType.GetHashCode());
                body.Emit(OpCodes.Bne_Un, exceptionLabel);
                knownTypes[secondType](body);
                body.Emit(OpCodes.Unbox_Any, genericParameter);
                body.Emit(OpCodes.Ret);

                return;
            }

            var halfLength = branch.Length / 2;
            var dividerCode = branch.Length % 2 == 0
                ? branch[halfLength].GetHashCode() - 1
                : branch[halfLength].GetHashCode();

            var @else = body.DefineLabel();

            body.Emit(OpCodes.Ldloc, hashCode);
            body.Emit(OpCodes.Ldc_I4, dividerCode);
            body.Emit(OpCodes.Bge, @else);

            var firstHalf = branch.Slice(0, halfLength);
            AppendBranch(firstHalf, body, genericParameter, knownTypes, hashCode, exceptionLabel);

            body.MarkLabel(@else);

            var secondHalf = branch.Slice(halfLength);
            AppendBranch(secondHalf, body, genericParameter, knownTypes, hashCode, exceptionLabel);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using StraightInject.Core.Debugging;
using StraightInject.Core.ServiceConstructors;

namespace StraightInject.Core
{
    internal class DynamicAssemblyTypeHandleJumpTableContainerCompiler : DynamicAssemblyContainerCompiler
    {
        public DynamicAssemblyTypeHandleJumpTableContainerCompiler(
            Dictionary<Type, IServiceCompiler> dependencyConstructors) : base(dependencyConstructors)
        {
        }

        protected override void AppendResolveMethodBody(ILGenerator body, Type genericParameter,
            Dictionary<Type, Action<ILGenerator>> knownTypes)
        {
            if (!knownTypes.Any())
            {
                body.Emit(OpCodes.Ldstr, "There is no provider for your service");
                var ctor = typeof(InvalidOperationException).GetConstructor(new[]
                {
                    typeof(string)
                });
                body.Emit(OpCodes.Newobj, ctor);
                body.Emit(OpCodes.Throw);
                return;
            }

            //todo: add logic to determine min shift size
            var startPointer = knownTypes.Keys.Min(type => type.TypeHandle.Value.ToInt64());
            var services = knownTypes.Keys.OrderBy(type => type.TypeHandle.Value.ToInt64())
                .ToDictionary(type => (int) (type.TypeHandle.Value.ToInt64() - startPointer) >> 6);

            if (DebugMode.Enabled())
            {
                Console.WriteLine("Pointers array :: ");
                foreach (var servicesKey in services.Keys)
                {
                    Console.WriteLine(servicesKey);
                }
            }

            var exceptionLabel = body.DefineLabel();

            var typeHandle = body.DeclareLocal(typeof(RuntimeTypeHandle));
            var typePointer = body.DeclareLocal(typeof(IntPtr));
            var typeHash = body.DeclareLocal(typeof(int));

            var getRuntimeTypePointer = typeof(RuntimeTypeHandle)
                .GetProperty("Value", BindingFlags.Public | BindingFlags.Instance).GetMethod;
            var toInt64 = typeof(IntPtr)
                .GetMethod("ToInt64", BindingFlags.Public | BindingFlags.Instance);


            //__typeref (T);
            body.Emit(OpCodes.Ldtoken, genericParameter);
            body.Emit(OpCodes.Stloc, typeHandle);

            //typeHandle.Value
            body.Emit(OpCodes.Ldloca, typeHandle);
            body.Emit(OpCodes.Call, getRuntimeTypePointer);
            body.Emit(OpCodes.Stloc, typePointer);

            //typePointer.ToInt64()
            body.Emit(OpCodes.Ldloca, typePointer);
            body.Emit(OpCodes.Call, toInt64);

            //typePointer.ToInt64() - start pointer
            body.Emit(OpCodes.Ldc_I8, startPointer);
            body.Emit(OpCodes.Sub);
            body.Emit(OpCodes.Conv_I4);

            //typeHashcode = (typePointer.ToInt64() - start pointer)pointer >> 7
            body.Emit(OpCodes.Ldc_I4_6);
            body.Emit(OpCodes.Shr);
            body.Emit(OpCodes.Stloc, typeHash);

            var lastKey = 0;
            var jumpLabels = new List<Label>();
            var jumpTable = new Dictionary<Type, Label>();

            foreach (var key in services.Keys)
            {
                var differential = key - lastKey;
                var label = body.DefineLabel();

                if (differential < 5)
                {
                    for (int i = 0; i < differential - 1; i++)
                    {
                        jumpLabels.Add(exceptionLabel);
                    }

                    jumpLabels.Add(label);
                }
                else
                {
                    body.Emit(OpCodes.Ldloc, typeHash);
                    body.Emit(OpCodes.Switch, jumpLabels.ToArray());

                    body.Emit(OpCodes.Ldloc, typeHash);
                    body.Emit(OpCodes.Ldc_I4, key);
                    body.Emit(OpCodes.Sub);
                    body.Emit(OpCodes.Stloc, typeHash);


                    jumpLabels = new List<Label>
                    {
                        label
                    };
                }

                jumpTable.Add(services[key], label);
                lastKey = key;
            }

            if (jumpLabels.Any())
            {
                body.Emit(OpCodes.Ldloc, typeHash);
                body.Emit(OpCodes.Switch, jumpLabels.ToArray());
            }

            body.Emit(OpCodes.Br, exceptionLabel);


            foreach (var (key, value) in jumpTable)
            {
                body.MarkLabel(value);
                knownTypes[key](body);
                body.Emit(OpCodes.Unbox_Any, genericParameter);
                body.Emit(OpCodes.Ret);
            }

            body.MarkLabel(exceptionLabel);
            body.Emit(OpCodes.Ldstr, "There is no provider for your service");
            var defaultConstructor = typeof(InvalidOperationException).GetConstructor(new[]
            {
                typeof(string)
            });
            body.Emit(OpCodes.Newobj, defaultConstructor);
            body.Emit(OpCodes.Throw);
        }
    }
}
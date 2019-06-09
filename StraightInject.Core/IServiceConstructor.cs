using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace StraightInject.Core
{
    internal interface IServiceConstructor
    {
        Action<ILGenerator> Construct(Type serviceType, IService service,
            Dictionary<Type, Action<ILGenerator>> knownTypes,
            Dictionary<Type, IService> dependencies,
            Stack<Type> constructionStack);
    }
}
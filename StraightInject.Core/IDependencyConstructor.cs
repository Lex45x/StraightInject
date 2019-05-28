using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace StraightInject.Core
{
    internal interface IDependencyConstructor
    {
        Action<ILGenerator> Construct(Type serviceType, IDependency dependency,
            Dictionary<Type, Action<ILGenerator>> knownTypes,
            Dictionary<Type, IDependency> dependencies,
            Stack<Type> constructionStack);
    }
}
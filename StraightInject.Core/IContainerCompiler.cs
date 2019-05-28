using System;
using System.Collections.Generic;

namespace StraightInject.Core
{
    internal interface IContainerCompiler
    {
        IContainer CompileDependencies(Dictionary<Type, IDependency> dependencies);
    }
}
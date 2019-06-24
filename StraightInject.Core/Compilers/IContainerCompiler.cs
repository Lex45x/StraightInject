using System;
using System.Collections.Generic;
using StraightInject.Services;

namespace StraightInject.Core.Compilers
{
    /// <summary>
    /// Compile a set of dependencies into a container
    /// </summary>
    internal interface IContainerCompiler
    {
        IContainer CompileDependencies(Dictionary<Type, IService> dependencies);
    }
}
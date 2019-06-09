using System;
using System.Collections.Generic;
using DynamicContainer;
using StraightInject.Services;

namespace StraightInject.Core
{
    internal interface IContainerCompiler
    {
        IContainer CompileDependencies(Dictionary<Type, IService> dependencies);
    }
}
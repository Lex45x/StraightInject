using System;
using System.Collections.Generic;

namespace StraightInject.Core.Compilers
{
    /// <summary>
    /// Accumulate the state of container before container instance activation
    /// </summary>
    public interface IContainerInitialState
    {
        /// <summary>
        /// Held an instances of singleton service
        /// </summary>
        Dictionary<Type, object> ServiceInstances { get; }
    }
}
using System;
using System.Collections.Generic;

namespace StraightInject.Core
{
    internal class TypeDependency : IDependency
    {
        public Type OriginalType { get; }

        public TypeDependency(Type originalType)
        {
            OriginalType = originalType;
        }
    }
}
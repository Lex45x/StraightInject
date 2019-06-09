using System;
using System.Collections.Generic;

namespace StraightInject.Core
{
    internal class TypedService : IService
    {
        public Type OriginalType { get; }

        public TypedService(Type originalType)
        {
            OriginalType = originalType;
        }
    }
}
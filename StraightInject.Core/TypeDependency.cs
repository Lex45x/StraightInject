using System;
using System.Collections.Generic;

namespace StraightInject.Core
{
    internal class TypeDependency : IDependency
    {
        public Type OriginalType { get; }
        private readonly Dictionary<Type, IDependency> dictionary;

        public TypeDependency(Type originalType, Dictionary<Type, IDependency> dictionary)
        {
            OriginalType = originalType;
            this.dictionary = dictionary;
        }

        public void SetServiceType<TService>()
        {
            dictionary.Add(typeof(TService), this);
        }
    }
}
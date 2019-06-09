using System;

namespace StraightInject.Core.Services
{
    internal class SingletonService : TypedService
    {
        public SingletonService(Type originalType, IConstructorResolver constructorResolver, Type serviceType) : base(
            originalType, constructorResolver, serviceType)
        {
        }
    }
}
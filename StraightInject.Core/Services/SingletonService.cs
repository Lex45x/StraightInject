using System;

namespace StraightInject.Core.Services
{
    /// <summary>
    /// Service that can have only single instance
    /// </summary>
    internal class SingletonService : TypedService
    {
        public SingletonService(Type originalType, IConstructorResolver constructorResolver, Type serviceType) : base(
            originalType, constructorResolver, serviceType)
        {
        }
    }
}
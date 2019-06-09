using System;
using System.Collections.Generic;
using System.Reflection;
using StraightInject.Services;

namespace StraightInject.Core.Services
{
    internal class TypedService : IConstructableService
    {
        private IConstructorResolver constructorResolver;
        public Type OriginalType { get; }

        public TypedService(Type originalType, IConstructorResolver constructorResolver, Type serviceType)
        {
            this.constructorResolver = constructorResolver;
            ServiceType = serviceType;
            OriginalType = originalType;
        }

        public ConstructorInfo GetConstructor(Dictionary<Type, IService> dependencies)
        {
            return constructorResolver.Resolve(OriginalType, dependencies);
        }

        public void OverrideConstructorResolver(IConstructorResolver resolver)
        {
            constructorResolver = resolver;
        }

        public Type ServiceType { get; }
    }
}
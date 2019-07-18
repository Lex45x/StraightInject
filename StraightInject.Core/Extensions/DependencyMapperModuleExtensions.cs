using System;

namespace StraightInject.Core.Extensions
{
    /// <summary>
    /// Extensions for <see cref="IDependencyMapper"/> interface. Provides an ability to compose services from <see cref="IModule"/>
    /// </summary>
    public static class DependencyMapperModuleExtensions
    {
        public static IDependencyMapper FromModule<TModule>(this IDependencyMapper dependencyMapper)
            where TModule : IModule
        {
            return FromModule(dependencyMapper, typeof(TModule));
        }

        public static IDependencyMapper FromModule<TModule>(this IDependencyMapper dependencyMapper,
            params object[] moduleConstructorParams) where TModule : IModule
        {
            return FromModule(dependencyMapper, typeof(TModule), moduleConstructorParams);
        }

        public static IDependencyMapper FromModule(this IDependencyMapper dependencyMapper, Type moduleType)
        {
            var module = CreateModule(moduleType);
            module.Apply(dependencyMapper);

            return dependencyMapper;
        }

        public static IDependencyMapper FromModule(this IDependencyMapper dependencyMapper, Type moduleType,
            params object[] moduleConstructorParams)
        {
            var module = CreateModule(moduleType, moduleConstructorParams);
            module.Apply(dependencyMapper);

            return dependencyMapper;
        }

        public static IDependencyMapper FromModule(this IDependencyMapper dependencyMapper, IModule module)
        {
            module.Apply(dependencyMapper);

            return dependencyMapper;
        }

        private static IModule CreateModule(Type moduleType, params object[] moduleConstructorParams)
        {
            if (!moduleType.IsPublic)
            {
                throw new InvalidOperationException("Unable to create non-public module");
            }

            var instance = Activator.CreateInstance(type: moduleType, args: moduleConstructorParams);

            return (IModule) instance;
        }
    }
}
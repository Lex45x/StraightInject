using System;
using System.Collections.Generic;
using System.Reflection;
using StraightInject.Services;

namespace StraightInject
{
    public interface IDependencyMapper
    {
        /// <summary>
        /// Create a new rule that allow to map <typeparamref name="T"/> as a specific service
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IComponentComposer<IConstructableService,TComponent> FromType<TComponent>();

        /// <summary>
        /// Same as <see cref="FromTyperomType{TComponent}"/> but with implicit type passing
        /// </summary>
        /// <param name="implementationType"></param>
        /// <returns></returns>
        IComponentComposer<IConstructableService> FromType(Type implementationType);

        /// <summary>
        /// Compile existing map to a Completed container
        /// </summary>
        /// <returns></returns>
        IContainer Compile();
    }

    public interface IComponentComposer<out TService> where TService : IService
    {
        TService ToService<TServiceType>();
        TService ToService(Type serviceType);
    }

    public interface IComponentComposer<out TService, TComponent> : IComponentComposer<TService>
        where TService : IService
    {
    }

    public interface IConstructorResolver
    {
        ConstructorInfo Resolve(Type component, Dictionary<Type, IService> dependencies);
    }
}
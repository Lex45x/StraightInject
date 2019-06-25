using System;
using StraightInject.Services;

namespace StraightInject
{
    /// <summary>
    /// Represent an entry point to configure specific component.
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    public interface IComponentComposer<out TService> where TService : IService
    {
        TService ToService<TServiceType>();
        TService ToService(Type serviceType);
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <typeparam name="TComponent"></typeparam>
    public interface IComponentComposer<out TService, TComponent> : IComponentComposer<TService>
        where TService : IService
    {
    }
}
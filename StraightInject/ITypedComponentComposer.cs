using StraightInject.Services;

namespace StraightInject
{
    /// <summary>
    /// Represent an entry point to configure a type-based component
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    public interface ITypedComponentComposer<out TService> : IComponentComposer<TService> where TService : IService
    {
        IComponentComposer<TService> SingleInstance();
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <typeparam name="TComponent"></typeparam>
    public interface ITypedComponentComposer<out TService, TComponent> : IComponentComposer<TService, TComponent>
        where TService : IService
    {
        IComponentComposer<TService> SingleInstance();
    }
}
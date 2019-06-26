using System;
using StraightInject.Services;

namespace StraightInject
{
    public interface IDependencyMapper
    {
        /// <summary>
        /// Create a new rule that allow to map <typeparamref name="TComponent"/> as a specific service
        /// </summary>
        /// <typeparam name="TComponent"></typeparam>
        /// <returns></returns>
        IComponentComposer<IConstructableService, TComponent> FromType<TComponent>();

        /// <summary>
        /// Same as <see cref="FromType{TComponent}"/> but with implicit type passing
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
}
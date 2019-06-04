using System;

namespace StraightInject
{
    public interface IDependencyMapper
    {
        /// <summary>
        /// Create a new rule that allow to map <typeparamref name="T"/> as a specific service
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IDependency<T> MapType<T>();

        IDependency MapType(Type implementationType);

        /// <summary>
        /// Compile existing map to a Completed container
        /// </summary>
        /// <returns></returns>
        IContainer Compile();
    }
}
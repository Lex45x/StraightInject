namespace StraightInject
{
    /// <summary>
    /// Represents IoC container access interface
    /// </summary>
    public interface IContainer
    {
        /// <summary>
        /// Return an instance of the service T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T Resolve<T>();
    }
}
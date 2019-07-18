namespace StraightInject
{
    /// <summary>
    /// Wrapps composing of several services into independent module
    /// </summary>
    public interface IModule
    {
        /// <summary>
        /// Applies composing of services for specified mapper
        /// </summary>
        /// <param name="dependencyMapper">DependencyMapper itself</param>
        void Apply(IDependencyMapper dependencyMapper);
    }
}
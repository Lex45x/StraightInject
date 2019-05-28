namespace StraightInject
{
    public interface IDependency
    {
        /// <summary>
        /// Used to set original service type. <typeparamref name="TOriginal"/> must be derived from <typeparamref name="TService"/> or implements <typeparamref name="TService"/>
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        void SetServiceType<TService>();
    }

    public interface IDependency<TOriginal> : IDependency
    {
    }
}
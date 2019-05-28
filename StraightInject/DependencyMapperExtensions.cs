namespace StraightInject
{
    public static class DependencyMapperExtensions
    {
        public static IDependency<TOriginal> As<TOriginal, TService>(this IDependency<TOriginal> dependency)
            where TOriginal : TService
        {
            dependency.SetServiceType<TService>();
            return dependency;
        }
    }
}
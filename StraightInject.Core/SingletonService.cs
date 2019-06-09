namespace StraightInject.Core
{
    internal class SingletonService : IService
    {
        public object Instance { get; }

        public SingletonService(object instance)
        {
            Instance = instance;
        }
    }
}
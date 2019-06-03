using StraightInject;

namespace DynamicContainer
{
    public class Container : IContainer
    {
        public T Resolve<T>()
        {
            throw new System.NotImplementedException();
        }
    }
}
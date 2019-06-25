namespace StraightInject.Core.Tests.Services
{
    public class MultiConstructorService
    {
        public IPlainService Service { get; }

        public MultiConstructorService()
        {
        }

        public MultiConstructorService(IPlainService plainService)
        {
            Service = plainService;
        }
    }
}
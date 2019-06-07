namespace StraightInject.Core.Tests.Services
{
    public class DependentService : IDependentService
    {
        private readonly IDependencyService dependency;

        public DependentService(IDependencyService dependency)
        {
            this.dependency = dependency;
        }
    }
}
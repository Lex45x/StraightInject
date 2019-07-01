using StraightInject.Core.Tests.Services.MVC.Configuration;

namespace StraightInject.Core.Tests.Services.MVC.DataAccess
{
    public class UnitOfWork : IUserRepository, IUserActivityRepository
    {
        public IDatabaseConfiguration Configuration { get; }

        public UnitOfWork()
        {
            Configuration = null;
        }

        public UnitOfWork(IDatabaseConfiguration configuration)
        {
            Configuration = configuration;
        }
    }
}
using StraightInject.Core.Tests.Services.MVC.Configuration;

namespace StraightInject.Core.Tests.Services.MVC.DataAccess
{
    public class UserAuthorizationDetailsCache : IUserAuthorizationDetailsCache
    {
        private readonly ICacheConfiguration configuration;

        public UserAuthorizationDetailsCache(ICacheConfiguration configuration)
        {
            this.configuration = configuration;
        }
    }
}
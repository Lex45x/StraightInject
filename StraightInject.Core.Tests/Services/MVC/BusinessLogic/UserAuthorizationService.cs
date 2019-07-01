using StraightInject.Core.Tests.Services.MVC.DataAccess;

namespace StraightInject.Core.Tests.Services.MVC.BusinessLogic
{
    public class UserAuthorizationService : IUserAuthorizationService
    {
        private readonly IUserAuthorizationDetailsCache cache;
        private readonly IUserRepository repository;

        public UserAuthorizationService(IUserAuthorizationDetailsCache cache, IUserRepository repository)
        {
            this.cache = cache;
            this.repository = repository;
        }
    }
}
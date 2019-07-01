using StraightInject.Core.Tests.Services.MVC.DataAccess;

namespace StraightInject.Core.Tests.Services.MVC.BusinessLogic
{
    public class UserService : IUserService
    {
        private readonly IUserRepository repository;

        public UserService(IUserRepository repository)
        {
            this.repository = repository;
        }
    }
}
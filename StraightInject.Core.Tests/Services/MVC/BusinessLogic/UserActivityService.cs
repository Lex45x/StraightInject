using StraightInject.Core.Tests.Services.MVC.DataAccess;

namespace StraightInject.Core.Tests.Services.MVC.BusinessLogic
{
    public class UserActivityService : IUserActivityService
    {
        private readonly IUserRepository userRepository;
        private readonly IUserActivityRepository userActivityRepository;

        public UserActivityService(IUserRepository userRepository, IUserActivityRepository userActivityRepository)
        {
            this.userRepository = userRepository;
            this.userActivityRepository = userActivityRepository;
        }
    }
}
using StraightInject.Core.Tests.Services.MVC.BusinessLogic;

namespace StraightInject.Core.Tests.Services.MVC.Controllers.Protected
{
    public class UserActivityController
    {
        private readonly IUserAuthorizationService authorizationService;
        private readonly IUserActivityService userActivityService;

        public UserActivityController(IUserAuthorizationService authorizationService,
            IUserActivityService userActivityService)
        {
            this.authorizationService = authorizationService;
            this.userActivityService = userActivityService;
        }
    }
}
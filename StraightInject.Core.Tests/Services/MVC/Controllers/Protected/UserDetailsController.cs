using StraightInject.Core.Tests.Services.MVC.BusinessLogic;

namespace StraightInject.Core.Tests.Services.MVC.Controllers.Protected
{
    public class UserDetailsController
    {
        private readonly IUserAuthorizationService authorizationService;
        private readonly IUserService userService;

        public UserDetailsController(IUserAuthorizationService authorizationService, IUserService userService)
        {
            this.authorizationService = authorizationService;
            this.userService = userService;
        }
    }
}
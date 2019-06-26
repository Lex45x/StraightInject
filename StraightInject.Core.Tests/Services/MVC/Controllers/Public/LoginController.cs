using StraightInject.Core.Tests.Services.MVC.BusinessLogic;
using StraightInject.Core.Tests.Services.MVC.ThirdParty;

namespace StraightInject.Core.Tests.Services.MVC.Controllers.Public
{
    public class LoginController
    {
        private readonly IFacebookIntegrationService facebookIntegrationService;
        private readonly IGoogleIntegrationService googleIntegrationService;
        private readonly IUserAuthorizationService userAuthorizationService;
        private readonly IUserService userService;

        public LoginController(IFacebookIntegrationService facebookIntegrationService,
            IGoogleIntegrationService googleIntegrationService, IUserAuthorizationService userAuthorizationService,
            IUserService userService)
        {
            this.facebookIntegrationService = facebookIntegrationService;
            this.googleIntegrationService = googleIntegrationService;
            this.userAuthorizationService = userAuthorizationService;
            this.userService = userService;
        }
    }
}
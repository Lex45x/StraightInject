using System.Net.Http;
using StraightInject.Core.Tests.Services.MVC.Configuration;

namespace StraightInject.Core.Tests.Services.MVC.ThirdParty
{
    public class FacebookIntegrationService : IFacebookIntegrationService
    {
        private readonly HttpClient client;
        private readonly IFacebookIntegrationConfiguration configuration;

        public FacebookIntegrationService(HttpClient client, IFacebookIntegrationConfiguration configuration)
        {
            this.client = client;
            this.configuration = configuration;
        }
    }
}
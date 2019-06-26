using System.Net.Http;
using StraightInject.Core.Tests.Services.MVC.Configuration;

namespace StraightInject.Core.Tests.Services.MVC.ThirdParty
{
    public class GoogleIntegrationService : IGoogleIntegrationService
    {
        private readonly HttpClient client;
        private readonly IGoogleIntegrationConfiguration configuration;

        public GoogleIntegrationService(HttpClient client, IGoogleIntegrationConfiguration configuration)
        {
            this.client = client;
            this.configuration = configuration;
        }
    }
}
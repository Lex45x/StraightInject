using System;
using System.Net.Http;
using Autofac;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using NUnit.Framework;
using StraightInject.Core.Tests.Services.MVC.BusinessLogic;
using StraightInject.Core.Tests.Services.MVC.Configuration;
using StraightInject.Core.Tests.Services.MVC.Controllers.Public;
using StraightInject.Core.Tests.Services.MVC.DataAccess;
using StraightInject.Core.Tests.Services.MVC.ThirdParty;

namespace StraightInject.Core.Tests.Benchmarks.Container
{
    public class LoginControllerResolutionBenchmarks : DependentServiceResolutionBenchmarksBase
    {
        [Test]
        public void MeasureDependentServiceResolutionBenchmarks()
        {
            var summary = BenchmarkRunner.Run<LoginControllerResolutionBenchmarks>();
        }

        [Benchmark(Baseline = true)]
        public LoginController RawInstantiate()
        {
            return new LoginController(
                new FacebookIntegrationService(new HttpClient(), new FacebookIntegrationConfiguration()),
                new GoogleIntegrationService(new HttpClient(), new GoogleIntegrationConfiguration()),
                new UserAuthorizationService(new UserAuthorizationDetailsCache(new CacheConfiguration()),
                    new UnitOfWork(new DatabaseConfiguration())),
                new UserService(new UnitOfWork(new DatabaseConfiguration())));
        }

        [Benchmark]
        public LoginController StraightInjectContainerInstantiate()
        {
            return straightInjectContainer.Resolve<LoginController>();
        }

        [Benchmark]
        public LoginController AutofacContainerInstantiate()
        {
            return autofacContainer.Resolve<LoginController>();
        }

        [Benchmark]
        public LoginController AbiocContainerInstantiate()
        {
            return abiocContainer.GetService<LoginController>();
        }
    }
}
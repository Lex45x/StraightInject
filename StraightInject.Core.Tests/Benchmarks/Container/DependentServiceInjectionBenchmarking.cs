using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Abioc;
using Abioc.Registration;
using Autofac;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using NUnit.Framework;
using StraightInject.Core.Compilers;
using StraightInject.Core.ServiceConstructors;
using StraightInject.Core.Services;
using StraightInject.Core.Tests.Services;
using StraightInject.Core.Tests.Services.EmptyServices;
using StraightInject.Core.Tests.Services.MVC.BusinessLogic;
using StraightInject.Core.Tests.Services.MVC.Configuration;
using StraightInject.Core.Tests.Services.MVC.Controllers.Protected;
using StraightInject.Core.Tests.Services.MVC.Controllers.Public;
using StraightInject.Core.Tests.Services.MVC.DataAccess;
using StraightInject.Core.Tests.Services.MVC.ThirdParty;

namespace StraightInject.Core.Tests.Benchmarks.Container
{
    [TestFixture]
    [SimpleJob(targetCount: 100)]
    [DisassemblyDiagnoser(printIL: true)]
    [MinColumn, MaxColumn, MeanColumn, MedianColumn, StdErrorColumn, StdDevColumn]
    public class DependentServiceResolutionBenchmarks
    {
        private readonly IContainer straightInjectContainer;

        private readonly Autofac.IContainer autofacContainer;

        private readonly Abioc.IContainer abiocContainer;

        static DependentServiceResolutionBenchmarks()
        {
            Registrations = new Dictionary<Type, Type>
            {
                [typeof(HttpClient)] = typeof(HttpClient),
                [typeof(IUserActivityService)] = typeof(UserActivityService),
                [typeof(IUserAuthorizationService)] = typeof(UserAuthorizationService),
                [typeof(IUserService)] = typeof(UserService),
                [typeof(ICacheConfiguration)] = typeof(CacheConfiguration),
                [typeof(IDatabaseConfiguration)] = typeof(DatabaseConfiguration),
                [typeof(IFacebookIntegrationConfiguration)] = typeof(FacebookIntegrationConfiguration),
                [typeof(IGoogleIntegrationConfiguration)] = typeof(GoogleIntegrationConfiguration),
                [typeof(UserActivityController)] = typeof(UserActivityController),
                [typeof(UserDetailsController)] = typeof(UserDetailsController),
                [typeof(IUserActivityRepository)] = typeof(UnitOfWork),
                [typeof(IUserAuthorizationDetailsCache)] = typeof(UserAuthorizationDetailsCache),
                [typeof(IUserRepository)] = typeof(UnitOfWork),
                [typeof(IFacebookIntegrationService)] = typeof(FacebookIntegrationService),
                [typeof(IGoogleIntegrationService)] = typeof(GoogleIntegrationService)
            };

            foreach (var service in Assembly.GetExecutingAssembly().ExportedTypes
                .Where(type => type.GetInterfaces().Contains(typeof(IEmptyService))))
            {
                Registrations.Add(service, service);
            }

            Registrations.Add(typeof(LoginController), typeof(LoginController));
        }

        private static readonly Dictionary<Type, Type> Registrations;

        public DependentServiceResolutionBenchmarks()
        {
            var mapperV1 = new DefaultDependencyComposer(
                new DynamicAssemblyJumpTableOfTypeHandleContainerCompiler(
                    new Dictionary<Type, IServiceCompiler>
                    {
                        [typeof(TypedService)] = new TypedServiceCompiler()
                    }));

            AddRegistrations(mapperV1);
            straightInjectContainer = mapperV1.Compile();

            var builder = new ContainerBuilder();
            AddRegistrations(builder);
            autofacContainer = builder.Build();

            var registrationSetup = new RegistrationSetup();
            AddRegistrations(registrationSetup);
            abiocContainer =
                registrationSetup.Construct(new[] {Assembly.GetExecutingAssembly(), typeof(HttpClient).Assembly},
                    out var code);
        }

        private static void AddRegistrations(RegistrationSetup registrationSetup)
        {
            foreach (var (key, value) in Registrations)
            {
                //Abioc can work with classes that have single constructor only =/
                if (value == typeof(HttpClient))
                {
                    registrationSetup.RegisterFactory(typeof(HttpClient), () => new HttpClient());
                }

                //things goes even worse if you have an hierarchy
                if (value == typeof(UnitOfWork))
                {
                    registrationSetup.RegisterFactory(key, () => new UnitOfWork(new DatabaseConfiguration()));
                }
                else
                {
                    registrationSetup.Register(key, value);
                }
            }
        }

        private static void AddRegistrations(ContainerBuilder builder)
        {
            foreach (var (key, value) in Registrations)
            {
                builder.RegisterType(value).As(key);
            }
        }

        private static void AddRegistrations(IDependencyMapper mapper)
        {
            foreach (var (key, value) in Registrations)
            {
                mapper.FromType(value).ToService(key);
            }
        }

        [Test]
        public void MeasureDependentServiceResolutionBenchmarks()
        {
            var summary = BenchmarkRunner.Run<DependentServiceResolutionBenchmarks>();
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
        public LoginController StraightIbjectContainerInstantiate()
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
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
    [SimpleJob(targetCount: 10)]
    [DisassemblyDiagnoser(printIL: true)]
    [MinColumn, MaxColumn, MeanColumn, MedianColumn, StdErrorColumn, StdDevColumn]
    public class DependentServiceResolutionBenchmarksBase
    {
        protected IContainer straightInjectContainer;
        protected Autofac.IContainer autofacContainer;
        protected Abioc.IContainer abiocContainer;
        private static Dictionary<Type, Type> Registrations;

        static DependentServiceResolutionBenchmarksBase()
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

        [GlobalSetup]
        [OneTimeSetUp]
        public void Setup()
        {
            var mapperV1 = DefaultDependencyComposer.Initialize();

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

        protected virtual void AddRegistrations(RegistrationSetup registrationSetup)
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

        protected virtual void AddRegistrations(ContainerBuilder builder)
        {
            foreach (var (key, value) in Registrations)
            {
                builder.RegisterType(value).As(key);
            }
        }

        protected virtual void AddRegistrations(IDependencyMapper mapper)
        {
            foreach (var (key, value) in Registrations)
            {
                mapper.FromType(value).ToService(key);
            }
        }
    }
}
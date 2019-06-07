using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Abioc;
using Abioc.Registration;
using Autofac;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using NUnit.Framework;
using StraightInject.Core.Tests.Services;
using StraightInject.Core.Tests.Services.EmptyServices;

namespace StraightInject.Core.Tests.Benchmarks.Container
{
    [TestFixture]
    [SimpleJob(targetCount: 100, invocationCount: 10, launchCount: 10)]
    [DisassemblyDiagnoser(printIL: true)]
    [MinColumn, MaxColumn, MeanColumn, MedianColumn, StdErrorColumn, StdDevColumn]
    public class DependentServiceResolutionBenchmarks
    {
        private readonly IContainer conceptContainerV1;

        private readonly Autofac.IContainer autofacContainer;

        private readonly Abioc.IContainer abiocContainer;
        private readonly IContainer conceptContainerV2;

        static DependentServiceResolutionBenchmarks()
        {
            Registrations = new Dictionary<Type, Type>
            {
                [typeof(IPlainService)] = typeof(PlainService),
                [typeof(IDependencyService)] = typeof(DependencyService),
                [typeof(object)] = typeof(object),
                [typeof(PlainService)] = typeof(PlainService),
                [typeof(DependentService)] = typeof(DependentService),
                [typeof(DependencyService)] = typeof(DependencyService),
                [typeof(MultiInterfaceService)] = typeof(MultiInterfaceService),
                [typeof(IMultiInterfaceService)] = typeof(MultiInterfaceService),
                [typeof(IDisposable)] = typeof(MultiInterfaceService),
                [typeof(IConvertible)] = typeof(MultiInterfaceService),
                [typeof(IComparable)] = typeof(MultiInterfaceService),
                [typeof(IFormattable)] = typeof(MultiInterfaceService),
                [typeof(ICloneable)] = typeof(MultiInterfaceService)
            };

            foreach (var service in Assembly.GetExecutingAssembly().ExportedTypes
                .Where(type => type.GetInterfaces().Contains(typeof(IEmptyService))))
            {
                Registrations.Add(service, service);
            }

            Registrations.Add(typeof(IDependentService), typeof(DependentService));
        }

        private static readonly Dictionary<Type, Type> Registrations;
        private readonly IContainer conceptContainerV3;

        public DependentServiceResolutionBenchmarks()
        {
            var mapperV1 = new DefaultDependencyMapper(
                new DynamicAssemblyContainerCompiler(
                    new Dictionary<Type, IDependencyConstructor>
                    {
                        [typeof(TypeDependency)] = new TypeDependencyConstructor()
                    }));

            AddRegistrations(mapperV1);
            conceptContainerV1 = mapperV1.Compile();

            var mapperV2 = new DefaultDependencyMapper(
                new DynamicAssemblyBinarySearchByHashCodeContainerCompiler(
                    new Dictionary<Type, IDependencyConstructor>
                    {
                        [typeof(TypeDependency)] = new TypeDependencyConstructor()
                    }));

            AddRegistrations(mapperV2);
            conceptContainerV2 = mapperV2.Compile();

            var mapperV3 = new DefaultDependencyMapper(
                new DynamicAssemblyBinarySearchByMetadataTokenContainerCompiler(
                    new Dictionary<Type, IDependencyConstructor>
                    {
                        [typeof(TypeDependency)] = new TypeDependencyConstructor()
                    }));

            AddRegistrations(mapperV3);
            conceptContainerV3 = mapperV3.Compile();

            var builder = new ContainerBuilder();
            AddRegistrations(builder);
            autofacContainer = builder.Build();

            var registrationSetup = new RegistrationSetup();
            AddRegistrations(registrationSetup);
            abiocContainer = registrationSetup.Construct(Assembly.GetExecutingAssembly(), out var code);
        }

        private static void AddRegistrations(RegistrationSetup registrationSetup)
        {
            foreach (var (key, value) in Registrations)
            {
                registrationSetup.Register(key, value);
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
                mapper.MapType(value).SetServiceType(key);
            }
        }

        [Test]
        public void MeasureDependentServiceResolutionBenchmarks()
        {
            var summary = BenchmarkRunner.Run<DependentServiceResolutionBenchmarks>();
        }

        [Benchmark(Baseline = true)]
        public IDependentService RawInstantiate()
        {
            return new DependentService(new DependencyService());
        }

        [Benchmark]
        public IDependentService ConceptContainerV1Instantiate()
        {
            return conceptContainerV1.Resolve<IDependentService>();
        }

        [Benchmark]
        public IDependentService ConceptContainerV2Instantiate()
        {
            return conceptContainerV2.Resolve<IDependentService>();
        }

        [Benchmark]
        public IDependentService ConceptContainerV3Instantiate()
        {
            return conceptContainerV3.Resolve<IDependentService>();
        }

        [Benchmark]
        public IDependentService AutofacContainerInstantiate()
        {
            return autofacContainer.Resolve<IDependentService>();
        }

        [Benchmark]
        public IDependentService AbiocContainerInstantiate()
        {
            return abiocContainer.GetService<IDependentService>();
        }
    }
}
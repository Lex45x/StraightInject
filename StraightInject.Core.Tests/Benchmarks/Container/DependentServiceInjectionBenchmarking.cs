using System;
using System.Collections.Generic;
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

namespace StraightInject.Core.Tests.Benchmarks.Container
{
    [TestFixture]
    [SimpleJob(targetCount: 100)]
    [DisassemblyDiagnoser(printIL: true)]
    [MinColumn, MaxColumn, MeanColumn, MedianColumn, StdErrorColumn, StdDevColumn]
    public class DependentServiceResolutionBenchmarks
    {
        private readonly IContainer conceptContainerV1;

        private readonly Autofac.IContainer autofacContainer;

        private readonly AbiocContainer abiocContainer;
        private readonly IContainer conceptContainerV2;

        private static readonly Dictionary<Type, Type> Registrations = new Dictionary<Type, Type>
        {
            [typeof(IPlainService)] = typeof(PlainService),
            [typeof(IDependentService)] = typeof(DependentService),
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
                new DynamicAssemblyBinarySearchContainerCompiler(
                    new Dictionary<Type, IDependencyConstructor>
                    {
                        [typeof(TypeDependency)] = new TypeDependencyConstructor()
                    }));

            AddRegistrations(mapperV2);
            conceptContainerV2 = mapperV2.Compile();

            var builder = new ContainerBuilder();
            AddRegistrations(builder);
            autofacContainer = builder.Build();

            var registrationSetup = new RegistrationSetup();
            AddRegistrations(registrationSetup);
            abiocContainer = registrationSetup.Construct(Assembly.GetExecutingAssembly());
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
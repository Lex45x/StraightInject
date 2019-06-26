using Abioc.Registration;
using Autofac;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using NUnit.Framework;

namespace StraightInject.Core.Tests.Benchmarks.Container
{
    public class InstanceResolutionBenchmarks : DependentServiceResolutionBenchmarksBase
    {
        [Test]
        public void MeasureInstanceResolutionBenchmarks()
        {
            var summary = BenchmarkRunner.Run<InstanceResolutionBenchmarks>();
        }

        private static readonly string Instance = "Some Instance of Reference type";

        protected override void AddRegistrations(RegistrationSetup registrationSetup)
        {
            base.AddRegistrations(registrationSetup);
            registrationSetup.RegisterFixed(Instance);
        }

        protected override void AddRegistrations(ContainerBuilder builder)
        {
            base.AddRegistrations(builder);
            builder.RegisterInstance(Instance).AsSelf();
        }

        protected override void AddRegistrations(IDependencyMapper mapper)
        {
            base.AddRegistrations(mapper);
            mapper.FromInstance(Instance).ToService<string>();
        }

        [Benchmark(Baseline = true)]
        public string RawInstantiate()
        {
            return Instance;
        }

        [Benchmark]
        public string StraightInjectContainerInstantiate()
        {
            return straightInjectContainer.Resolve<string>();
        }

        [Benchmark]
        public string AutofacContainerInstantiate()
        {
            return autofacContainer.Resolve<string>();
        }

        [Benchmark]
        public string AbiocContainerInstantiate()
        {
            return abiocContainer.GetService<string>();
        }
    }
}
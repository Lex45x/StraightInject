using Autofac;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using NUnit.Framework;
using StraightInject.Core.Tests.Services;

namespace StraightInject.Core.Tests.Benchmarking
{
    [TestFixture]
    [DisassemblyDiagnoser(printIL: true, recursiveDepth: 10)]
    [MinColumn, MaxColumn, MeanColumn, MedianColumn, StdErrorColumn, StdDevColumn]
    public class DependencyInjectionBenchmarking
    {
        private readonly IContainer container;

        private readonly Autofac.IContainer autofacContainer;

        public DependencyInjectionBenchmarking()
        {
            var mapper = DefaultDependencyMapper.Initialize();
            mapper.MapType<PlainService>().SetServiceType<IPlainService>();
            mapper.MapType<DependentService>().SetServiceType<IDependentService>();
            mapper.MapType<DependencyService>().SetServiceType<IDependencyService>();
            container = mapper.Compile();

            var builder = new ContainerBuilder();
            builder.RegisterType<PlainService>().AsImplementedInterfaces();
            builder.RegisterType<DependentService>().AsImplementedInterfaces();
            builder.RegisterType<DependencyService>().AsImplementedInterfaces();
            autofacContainer = builder.Build();
        }

        [Test]
        public void BenchmarkingRun()
        {
            var summary = BenchmarkRunner.Run<DependencyInjectionBenchmarking>();
        }

        [Benchmark(Baseline = true)]
        public IPlainService RawInstantiate()
        {
            return new PlainService();
        }

        [Benchmark]
        public IPlainService ContainerInstantiate()
        {
            return container.Resolve<IPlainService>();
        }

        [Benchmark]
        public IPlainService AutofacContainerInstantiate()
        {
            return autofacContainer.Resolve<IPlainService>();
        }
    }
}
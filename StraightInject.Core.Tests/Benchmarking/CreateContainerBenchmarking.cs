using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using NUnit.Framework;
using StraightInject.Core.Tests.Services;

namespace StraightInject.Core.Tests.Benchmarking
{
    [TestFixture]
    public class CreateContainerBenchmarking
    {
        private readonly DefaultDependencyMapper mapper;

        public CreateContainerBenchmarking()
        {
            mapper = DefaultDependencyMapper.Initialize();
            mapper.MapType<PlainService>().SetServiceType<IPlainService>();
            mapper.MapType<DependentService>().SetServiceType<IDependentService>();
            mapper.MapType<DependencyService>().SetServiceType<IDependencyService>();
        }

        [Test]
        public void BenchmarkingRun()
        {
            var summary = BenchmarkRunner.Run<CreateContainerBenchmarking>();
        }

        [Benchmark]
        public IContainer CreateContainer()
        {
            return mapper.Compile();
        }
    }
}
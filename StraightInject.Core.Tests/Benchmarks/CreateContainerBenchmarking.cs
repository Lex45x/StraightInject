using BenchmarkDotNet.Attributes;
using StraightInject.Core.Tests.Services;

namespace StraightInject.Core.Tests.Benchmarks
{
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

        [Benchmark]
        public IContainer CreateContainer()
        {
            return mapper.Compile();
        }
    }
}
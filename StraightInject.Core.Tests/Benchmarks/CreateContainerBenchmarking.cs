using BenchmarkDotNet.Attributes;
using StraightInject.Core.Tests.Services;

namespace StraightInject.Core.Tests.Benchmarks
{
    public class CreateContainerBenchmarking
    {
        private readonly DefaultDependencyComposer composer;

        public CreateContainerBenchmarking()
        {
            composer = DefaultDependencyComposer.Initialize();
            composer.FromType<PlainService>().ToService<IPlainService>();
            composer.FromType<DependentService>().ToService<IDependentService>();
            composer.FromType<DependencyService>().ToService<IDependencyService>();
        }

        [Benchmark]
        public IContainer CreateContainer()
        {
            return composer.Compile();
        }
    }
}
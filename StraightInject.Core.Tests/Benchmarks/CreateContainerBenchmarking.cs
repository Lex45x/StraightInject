using BenchmarkDotNet.Attributes;
using StraightInject.Core.Tests.Services;
using StraightInject.Core.Tests.Services.MVC.Configuration;
using StraightInject.Core.Tests.Services.MVC.DataAccess;

namespace StraightInject.Core.Tests.Benchmarks
{
    public class CreateContainerBenchmarking
    {
        private readonly DefaultDependencyComposer composer;

        public CreateContainerBenchmarking()
        {
            composer = DefaultDependencyComposer.Initialize();
            composer.FromType<CacheConfiguration>().ToService<ICacheConfiguration>();
            composer.FromType<DatabaseConfiguration>().ToService<IDatabaseConfiguration>();
            composer.FromType<UnitOfWork>().ToService<IUserRepository>();
        }

        [Benchmark]
        public IContainer CreateContainer()
        {
            return composer.Compile();
        }
    }
}
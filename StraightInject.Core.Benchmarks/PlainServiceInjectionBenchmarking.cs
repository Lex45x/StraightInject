using System.Reflection;
using Abioc;
using Abioc.Registration;
using Autofac;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using StraightInject.Core.Benchmarks.Services;

namespace StraightInject.Core.Benchmarks
{
    [DisassemblyDiagnoser(printIL: true, recursiveDepth: 10)]
    [MinColumn, MaxColumn, MeanColumn, MedianColumn, StdErrorColumn, StdDevColumn]
    public class PlainServiceInjectionBenchmarking
    {
        private readonly IContainer container;

        private readonly Autofac.IContainer autofacContainer;

        public readonly AbiocContainer abiocContainer;

        public PlainServiceInjectionBenchmarking()
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

            var registrationSetup = new RegistrationSetup();
            registrationSetup.Register<IPlainService, PlainService>();
            registrationSetup.Register<IDependentService, DependentService>();
            registrationSetup.Register<IDependencyService, DependencyService>();
            abiocContainer = registrationSetup.Construct(Assembly.GetExecutingAssembly());
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

        [Benchmark]
        public IPlainService AbiocContainerInstantiate()
        {
            return abiocContainer.GetService<IPlainService>();
        }

        [Benchmark]
        public IPlainService CompiledContainerInstantiate()
        {
            return abiocContainer.GetService<IPlainService>();
        }
    }
}
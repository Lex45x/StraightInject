using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Abioc;
using Abioc.Registration;
using Autofac;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using StraightInject.Core.Benchmarks.Services;

namespace StraightInject.Core.Benchmarks
{
    [SimpleJob(targetCount: 10)]
    [DisassemblyDiagnoser(printIL: true)]
    [MinColumn, MaxColumn, MeanColumn, MedianColumn, StdErrorColumn, StdDevColumn]
    public class DependentServiceInjectionBenchmarking
    {
        private readonly IContainer conceptContainer;

        private readonly ContainerBase preCompailedContainer = new Container();

        private readonly Autofac.IContainer autofacContainer;

        private readonly AbiocContainer abiocContainer;

        public DependentServiceInjectionBenchmarking()
        {
            var mapper = DefaultDependencyMapper.Initialize();
            mapper.MapType<PlainService>().SetServiceType<IPlainService>();
            mapper.MapType<DependentService>().SetServiceType<IDependentService>();
            mapper.MapType<DependencyService>().SetServiceType<IDependencyService>();
            conceptContainer = mapper.Compile();

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
        public IDependentService RawInstantiate()
        {
            return new DependentService(new DependencyService());
        }

        [Benchmark]
        public IDependentService CallRawInstantiate()
        {
            return RawInstantiate();
        }

        [Benchmark]
        public IDependentService ConceptContainerInstantiate()
        {
            return conceptContainer.Resolve<IDependentService>();
        }

        [Benchmark]
        public IDependentService PreCompiledContainerInstantiate()
        {
            return preCompailedContainer.Resolve<DependentService>();
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


    public abstract class ContainerBase : IContainer
    {
        public abstract T Resolve<T>();
    }

    public sealed class Container : ContainerBase
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override T Resolve<T>()
        {
            if (typeof(T) == typeof(PlainService))
            {
                return (T)(object)new PlainService();
            }

            if (typeof(T) == typeof(DependencyService))
            {
                return (T)(object)new DependencyService();
            }

            if (typeof(T) == typeof(DependentService))
            {
                return (T)(object)new DependentService(new DependencyService());
            }

            throw new InvalidOperationException("There is no provider for your service");
        }
    }
}
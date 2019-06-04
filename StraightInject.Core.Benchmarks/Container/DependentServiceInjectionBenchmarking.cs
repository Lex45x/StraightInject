using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Abioc;
using Abioc.Registration;
using Autofac;
using BenchmarkDotNet.Attributes;
using StraightInject.Core.Benchmarks.Services;

namespace StraightInject.Core.Benchmarks.Container
{
    [SimpleJob(targetCount: 10)]
    [DisassemblyDiagnoser(printIL: true)]
    [MinColumn, MaxColumn, MeanColumn, MedianColumn, StdErrorColumn, StdDevColumn]
    public class DependentServiceResolutionBenchmarking
    {
        private readonly IContainer conceptContainer;

        private readonly Container preCompiledContainer = new Container();
        private readonly IContainer dictionaryContainer = new DictionaryBasedContainer();

        private readonly Autofac.IContainer autofacContainer;

        private readonly AbiocContainer abiocContainer;

        public DependentServiceResolutionBenchmarking()
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
            return preCompiledContainer.Resolve<IDependentService>();
        }

        [Benchmark]
        public IDependentService DictionaryBasedContainerInstantiate()
        {
            return dictionaryContainer.Resolve<IDependentService>();
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

    public sealed class Container : IContainer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Resolve<T>()
        {
            if (typeof(T) == typeof(IPlainService))
            {
                return (T) (object) new PlainService();
            }

            if (typeof(T) == typeof(IDependencyService))
            {
                return (T) (object) new DependencyService();
            }

            if (typeof(T) == typeof(IDependentService))
            {
                return (T) (object) new DependentService(new DependencyService());
            }

            throw new InvalidOperationException("There is no provider for your service");
        }
    }

    public sealed class DictionaryBasedContainer : IContainer
    {
        private static readonly Dictionary<Type, Func<object>> Factories = new Dictionary<Type, Func<object>>
        {
            [typeof(IPlainService)] = () => new PlainService(),
            [typeof(IDependencyService)] = () => new DependencyService(),
            [typeof(IDependentService)] = () => new DependentService(new DependencyService())
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Resolve<T>()
        {
            return (T) Factories[typeof(T)]();
        }
    }

    public sealed class BinarySearchContainer : IContainer
    {
        public T Resolve<T>()
        {
            throw new NotImplementedException();
        }
    }
}
﻿using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using StraightInject.Core.Benchmarks.Services;

namespace StraightInject.Core.Benchmarks
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
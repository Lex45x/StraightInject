using System;
using BenchmarkDotNet.Running;
using StraightInject.Core.Benchmarks.Container;

namespace StraightInject.Core.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<DependentServiceResolutionBenchmarking>();
        }
    }
}
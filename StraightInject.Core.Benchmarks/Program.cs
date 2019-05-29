using System;
using BenchmarkDotNet.Running;

namespace StraightInject.Core.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<DependentServiceInjectionBenchmarking>();
        }
    }
}
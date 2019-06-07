using System;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using NUnit.Framework;

namespace StraightInject.Core.Tests.Benchmarks.Clr
{
    [TestFixture]
    
    public class TypeComparisonBenchmark
    {
        private static readonly Type TestType = typeof(TypeComparisonBenchmark);
        private static readonly int TestTypeHashCode = typeof(TypeComparisonBenchmark).GetHashCode();

        [Test]
        public void RunBenchmarking()
        {
            var summary = BenchmarkRunner.Run<TypeComparisonBenchmark>();
        }

        [Benchmark]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public bool ClrEquals()
        {
            return typeof(TypeComparisonBenchmark) == TestType;
        }

        [Benchmark]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public bool ClrEqualsWithoutGetRealtimeType()
        {
            return TestType == TestType;
        }

        [Benchmark]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public bool HashCodeEquals()
        {
            return typeof(TypeComparisonBenchmark).GetHashCode() == TestTypeHashCode;
        }
    }
}
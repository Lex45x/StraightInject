﻿using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using StraightInject.Core.Tests.Services;
using StraightInject.Core.Tests.Services.MVC.Configuration;

namespace StraightInject.Core.Tests.Benchmarks.Clr
{
    [SimpleJob(targetCount: 10)]
    [DisassemblyDiagnoser(printIL: true)]
    [MinColumn, MaxColumn, MeanColumn, MedianColumn, StdErrorColumn, StdDevColumn]
    public class DictionariesBenchmark
    {
        private readonly Dictionary<Type, Func<object>> dictionary;
        private readonly SortedDictionary<int, Func<object>> sortedDictionary;

        public DictionariesBenchmark()
        {
            dictionary = new Dictionary<Type, Func<object>>
            {
                [typeof(object)] = () => new object(),
                [typeof(string)] = () => string.Empty,
                [typeof(CacheConfiguration)] = () => new CacheConfiguration(),
                [typeof(int)] = () => int.MaxValue
            };
            sortedDictionary = new SortedDictionary<int, Func<object>>(dictionary.ToDictionary(pair => pair.Key.GetHashCode(),pair => pair.Value));
        }

        [Benchmark]
        public Func<object> GetFactoryFromDictionary()
        {
            return dictionary[typeof(CacheConfiguration)];
        }

        [Benchmark]
        public Func<object> GetFactoryFromSortedDictionary()
        {
            return sortedDictionary[typeof(CacheConfiguration).GetHashCode()];
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Tanka.GraphQL.Language;
using Tanka.GraphQL.Language.Internal;

namespace Tanka.GraphQL.Benchmarks
{
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn]
    [MemoryDiagnoser]
    [MarkdownExporterAttribute.GitHub]
    public class MethodBenchmarks
    {
        [GlobalSetup]
        public void Setup()
        {
            NameCount = 10_000;
            Data = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes("test test test"));
        }

        public ReadOnlyMemory<byte> Data { get; set; }

        public int NameCount { get; set; }

        [Benchmark(Baseline = true)]
        public void Base()
        {
            for (int i = 0; i < NameCount; i++)
            {
                var str = Encoding.UTF8.GetString(Data.Span);
            }
        }

        [Benchmark()]
        public void Cached()
        {
            var str = string.Empty;
            for (int i = 0; i < NameCount; i++)
            {
                if (str == string.Empty)
                    str = Encoding.UTF8.GetString(Data.Span);
            }
        }
    }
}
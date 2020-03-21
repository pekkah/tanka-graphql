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
            const int uniqueNames = 1000;
            List<string> possibleNames = new List<string>(uniqueNames);

            int i = 0;
            for (; i < uniqueNames; i++)
            {
                possibleNames.Add($"name_{i:####}");
            }

            Writer = new BufferWriter(NameCount * Encoding.UTF8.GetByteCount("name_xxxx")+1);
            for (i = 0; i < 10; i++)
            {
                for (var x = 0; x < 1000; x++)
                {
                    Writer.Write(Encoding.UTF8.GetBytes(possibleNames[i]));
                    Writer.Write(Constants.NewLineMemory.Span);
                }
            }
            
        }

        public int NameCount { get; set; }

        public BufferWriter Writer { get; set; }

        [GlobalCleanup]
        public void Clean()
        {
            Writer.Dispose();
        }

        [Benchmark(Baseline = true)]
        public void Base()
        {
            var parser = Language.Parser.Create(Writer.WrittenSpan);

            for(int i =0; i < NameCount; i++)
            {
                parser.ParseName();
            }
        }
    }
}
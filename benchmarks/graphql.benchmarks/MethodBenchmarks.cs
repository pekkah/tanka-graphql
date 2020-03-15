using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace Tanka.GraphQL.Benchmarks
{
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn]
    [MemoryDiagnoser]
    [MarkdownExporterAttribute.GitHub]
    public class MethodBenchmarks
    {
        [Benchmark(Baseline = true)]
        public void Base()
        {
            var parser = Language.Parser.Create("name");
            var name = parser.ParseName();
        }

        [Benchmark]
        public void Alt()
        {
            
        }
    }
}
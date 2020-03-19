using System;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace Tanka.GraphQL.Benchmarks.RealWorldSchemas
{
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn]
    [MemoryDiagnoser]
    [MarkdownExporterAttribute.GitHub]
    public class ParseGitHubSchemaBenchmark
    {
        public byte[] GitHubBytes { get; private set; }

        [GlobalSetup]
        public void Setup()
        {
            GitHubBytes = File.ReadAllBytes("RealWorldSchemas/github.graphql");
        }

        [Benchmark]
        public void GitHub()
        {
            var parser = Language.Parser.Create(GitHubBytes);
            var typeSystem = parser.ParseTypeSystemDocument();

            if (typeSystem.TypeDefinitions == null || !typeSystem.TypeDefinitions.Any())
            {
                throw new Exception("It has types");
            }
        }
    }
}
using System;
using System.Linq;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using GraphQLParser;
using Tanka.GraphQL.Introspection;
using Tanka.GraphQL.Language;
using Lexer = GraphQLParser.Lexer;

namespace Tanka.GraphQL.Benchmarks
{
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn]
    [MemoryDiagnoser]
    [MarkdownExporterAttribute.GitHub]
    public class LanguageParserBenchmarks
    {
        public Memory<byte> IntrospectionQueryMemory;
        public string IntrospectionQuery { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            IntrospectionQuery = Introspect.DefaultQuery;
            IntrospectionQueryMemory = new Memory<byte>(Encoding.UTF8.GetBytes(IntrospectionQuery));
            var _ = Constants.Space;
        }

        [Benchmark(Baseline = true)]
        public void GraphQL_dotnet_Parser_IntrospectionQuery()
        {
            var parser = new GraphQLParser.Parser(new Lexer());
            var source = new Source(IntrospectionQuery);
            var document = parser.Parse(source);

            if (!document.Definitions.Any())
                throw new InvalidOperationException("Failed");
        }

        [Benchmark]
        public void Tanka_GraphQL_Parser_IntrospectionQuery()
        {
            var parser = Language.Parser.Create(IntrospectionQueryMemory.Span);
            var document = parser.ParseDocument();

            if (document.OperationDefinitions == null || !document.OperationDefinitions.Any())
                throw new InvalidOperationException("Failed");
        }
    }
}
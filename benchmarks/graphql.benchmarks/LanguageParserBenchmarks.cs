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
        public string SimpleQuery { get; set; }

        public string IntrospectionQuery { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            IntrospectionQuery = Introspect.DefaultQuery;
            IntrospectionQueryMemory = new Memory<byte>(Encoding.UTF8.GetBytes(IntrospectionQuery));
            var _ = Constants.Space;
        }

        [Benchmark(Baseline = true)]
        public void OldParser_IntrospectionQuery()
        {
            var parser = new GraphQLParser.Parser(new Lexer());
            var source = new Source(IntrospectionQuery);
            var document = parser.Parse(source);

            if (!document.Definitions.Any())
                throw new InvalidOperationException("Failed");
        }

        [Benchmark]
        public void NewParser_IntrospectionQuery_String()
        {
            var parser = Language.Parser.Create(IntrospectionQuery);
            var document = parser.ParseDocument();

            if (document.OperationDefinitions == null || !document.OperationDefinitions.Any())
                throw new InvalidOperationException("Failed");
        }

        [Benchmark]
        public void NewParser_IntrospectionQuery_Span()
        {
            var parser = Language.Parser.Create(IntrospectionQueryMemory.Span);
            var document = parser.ParseDocument();

            if ((document.OperationDefinitions == null) || !document.OperationDefinitions.Any())
                throw new InvalidOperationException("Failed");
        }
    }
}
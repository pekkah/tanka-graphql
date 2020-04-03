using System;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using GraphQLParser;
using Tanka.GraphQL.Introspection;

namespace Tanka.GraphQL.Benchmarks
{
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn]
    [MemoryDiagnoser]
    [MarkdownExporterAttribute.GitHub]
    public class LanguageLexerBenchmarks
    {
        public string SimpleQuery { get; set; }

        public string IntrospectionQuery { get; set; }

        public Memory<byte> IntrospectionQueryMemory { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            IntrospectionQuery = Introspect.DefaultQuery;
            IntrospectionQueryMemory = new Memory<byte>(Encoding.UTF8.GetBytes(IntrospectionQuery));
            SimpleQuery = "query { field }";
        }

        [Benchmark(Baseline = true)]
        public void GraphQL_dotnet_Lexer_IntrospectionQuery()
        {
            var lexer = new Lexer();
            var source = new Source(IntrospectionQuery);
            var token = lexer.Lex(source);
            while (token.Kind != TokenKind.EOF) token = lexer.Lex(source, token.End);
        }

        [Benchmark]
        public void Tanka_GraphQL_Lexer_IntrospectionQuery()
        {
            var lexer = Language.Lexer.Create(IntrospectionQueryMemory.Span);
            while (lexer.Advance())
            {
                //noop
            }
        }
    }
}
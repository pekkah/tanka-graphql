using BenchmarkDotNet.Attributes;
using GraphQLParser;
using Tanka.GraphQL.Introspection;
using Tanka.GraphQL.Language;
using Lexer = Tanka.GraphQL.Language.Lexer;
using TokenKind = GraphQLParser.TokenKind;

namespace Tanka.GraphQL.Benchmarks
{
    [MarkdownExporterAttribute.GitHub()]
    public class ParserBenchmarks
    {
        [GlobalSetup]
        public void Setup()
        {
            IntrospectionQuery = Introspect.DefaultQuery;
            var _ = Constants.Space;
        }

        public string IntrospectionQuery { get; set; }

        [Benchmark(Baseline = true)]
        public void OldLexer_IntrospectionQuery()
        {
            var lexer = new GraphQLParser.Lexer();
            var source = new Source(IntrospectionQuery);
            var token = lexer.Lex(source);
            while(token.Kind != TokenKind.EOF)
            {
                token = lexer.Lex(source, token.End);
            } 
        }

        [Benchmark]
        public void NewLexer_IntrospectionQuery()
        {
            var lexer = Lexer.Create(IntrospectionQuery);
            while (lexer.Advance())
            {
                //noop
            }
        }
    }
}
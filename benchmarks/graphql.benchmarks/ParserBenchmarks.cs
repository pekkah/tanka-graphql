using System;
using System.Linq;
using BenchmarkDotNet.Attributes;
using GraphQLParser;
using Tanka.GraphQL.Introspection;
using Tanka.GraphQL.Language;
using Lexer = GraphQLParser.Lexer;
using TokenKind = GraphQLParser.TokenKind;

namespace Tanka.GraphQL.Benchmarks
{
    [MarkdownExporterAttribute.GitHub]
    public class LanguageParserBenchmarks
    {
        public string SimpleQuery { get; set; }

        public string IntrospectionQuery { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            IntrospectionQuery = Introspect.DefaultQuery;
            SimpleQuery = "query { field }";
            var _ = Constants.Space;
        }

        [Benchmark(Baseline = true)]
        public void OldParser_SimpleQuery()
        {
            var parser = new GraphQLParser.Parser(new Lexer());
            var source = new Source(SimpleQuery);
            var document = parser.Parse(source);

            if (!document.Definitions.Any())
                throw new InvalidOperationException("Failed");
        }

        [Benchmark]
        public void NewParser_SimpleQuery()
        {
            var parser = Language.Parser.Create(SimpleQuery);
            var document = parser.ParseDocument();

            if (!document.OperationDefinitions.Any())
                throw new InvalidOperationException("Failed");
        }
    }

    [MarkdownExporterAttribute.GitHub]
    public class LanguageLexerBenchmarks
    {
        public string SimpleQuery { get; set; }

        public string IntrospectionQuery { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            IntrospectionQuery = Introspect.DefaultQuery;
            SimpleQuery = "query { field }";
            var _ = Constants.Space;
        }

        [Benchmark(Baseline = true)]
        public void OldLexer_IntrospectionQuery()
        {
            var lexer = new Lexer();
            var source = new Source(IntrospectionQuery);
            var token = lexer.Lex(source);
            while (token.Kind != TokenKind.EOF) token = lexer.Lex(source, token.End);
        }

        [Benchmark]
        public void NewLexer_IntrospectionQuery()
        {
            var lexer = Language.Lexer.Create(IntrospectionQuery);
            while (lexer.Advance())
            {
                //noop
            }
        }
    }
}
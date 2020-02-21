using CommandLine;

namespace Tanka.GraphQL.Generator.Tool
{
    [Verb("gen-model")]
    public class GenerateCommandOptions
    {
        [Option('f', "file", Required = true, HelpText = "Input file")]
        public string InputFile { get; set; } = string.Empty;

        [Option('o', "output", Required = true, HelpText = "Output file")]
        public string OutputFile { get; set; } = string.Empty;

        [Option('n', "namespace", HelpText = "Namespace")]
        public string Namespace { get; set; } = string.Empty;
    }
}
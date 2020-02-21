using System;
using System.IO;
using System.Threading.Tasks;
using CommandLine;
using Tanka.GraphQL.Generator.Core;

namespace Tanka.GraphQL.Generator.Tool
{
    public class Program
    {
        private static async Task<int> Main(string[] args)
        {
            var result = CommandLine.Parser.Default.ParseArguments<GenerateCommandOptions>(args);
            var retCode = await result.MapResult(
                RunGenerateCommand,
                _ => Task.FromResult(1));

            return retCode;
        }

        private static async Task<int> RunGenerateCommand(GenerateCommandOptions opts)
        {
            try
            {
                var input = Path.GetFullPath(opts.InputFile);
                var output = Path.GetFullPath(opts.OutputFile);

                if (!File.Exists(input))
                {
                    throw new FileNotFoundException(
                        $"Input GraphQL file not found", input);
                }

                var generator = new CodeGenerator(input, opts.Namespace);
                var unit = await generator.Generate();
                var sourceText = unit.ToFullString();

                Directory.CreateDirectory(Path.GetDirectoryName(output));
                await File.WriteAllTextAsync(output, sourceText);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
                return 1;
            }


            return 0;
        }
    }
}
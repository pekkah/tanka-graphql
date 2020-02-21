using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Tanka.GraphQL.Generator
{
    public class SchemaGenerator : Task
    {
        [Required] public string Command { get; set; } = string.Empty;

        public string? CommandArgs { get; set; }

        [Required] public string RootNamespace { get; set; } = string.Empty;

        public bool Force { get; set; }

        [Required] public ITaskItem[] InputFiles { get; set; } = new ITaskItem[0];

        [Output] public ITaskItem[] OutputFiles { get; set; } = new ITaskItem[0];

        public override bool Execute()
        {
            if (InputFiles == null)
                return true;

            var outputFiles = new List<ITaskItem>();
            foreach (var inputFile in InputFiles)
            {
                var output = RunGenerator(inputFile);

                if (output == null)
                    return false;

                outputFiles.Add(output);
            }

            OutputFiles = outputFiles.ToArray();

            return true;
        }

        private ITaskItem? RunGenerator(ITaskItem inputFile)
        {
            var inputFilePath = Path.GetFullPath(inputFile.ItemSpec);
            var outputItemSpec = inputFile.GetMetadata("Code");

            if (!File.Exists(inputFilePath))
                return null;

            if (string.IsNullOrEmpty(outputItemSpec))
            {
                Log.LogError($"Item {inputFile} is missing 'Code' metadata entry. Cannot generate code.");
                return null;
            }

            var outputFilePath = outputItemSpec;

            if (Path.IsPathRooted(outputFilePath))
                outputFilePath = Path.GetFullPath(outputFilePath);

            // caching
            if (!Force && File.Exists(outputFilePath))
            {
                var lastModified = File.GetLastWriteTimeUtc(inputFilePath);
                var lastGenerated = File.GetLastWriteTimeUtc(outputFilePath);

                // if file hasn't been changed since last time generated
                if (lastModified < lastGenerated)
                {
                    Log.LogMessage(
                        MessageImportance.High,
                        $"Input file '{inputFilePath}' hasn't changed since last time generated");

                    // return existing file
                    return new TaskItem(outputFilePath);
                }
            }

            Log.LogMessage($"In: {inputFilePath}");
            Log.LogMessage($"Out: {outputFilePath}");

            var command = Command.Trim();
            var args = GetCommandArgs(inputFilePath, outputFilePath, ToNamespace(inputFile.ItemSpec));
            Log.LogCommandLine($"{command} {args}");

            if (!RunGeneratorCommand(command, args))
                return null;

            return new TaskItem(outputFilePath);
        }

        private bool RunGeneratorCommand(string command, string args)
        {
            try
            {
                using var process = Process.Start(new ProcessStartInfo(command)
                {
                    Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                });

                if (process == null)
                    return false;

                process.WaitForExit();

                var output = process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();

                if (!string.IsNullOrEmpty(output))
                    Log.LogMessage(output);

                if (!string.IsNullOrEmpty(error))
                    Log.LogError(error);

                if (process.ExitCode > 0)
                    return false;

                return true;
            }
            catch (Exception e)
            {
                Log.LogError($"Failed to execute '{command} {args}'");
                Log.LogErrorFromException(e);
                return false;
            }
        }

        private string ToNamespace(string itemSpec)
        {
            var dir = Path.GetDirectoryName(itemSpec);

            if (dir == null)
                throw new InvalidOperationException($"Could not get directory name from '{itemSpec}'.");

            return Regex.Replace(dir, "\\s+", "")
                .Replace(Path.DirectorySeparatorChar, '.')
                .Replace(Path.AltDirectorySeparatorChar, '.');
        }

        private string GetCommandArgs(string inputFilePath, string outputFilePath, string itemNamespace)
        {
            var builder = new StringBuilder();

            // add args if given
            if (!string.IsNullOrEmpty(CommandArgs))
            {
                builder.Append(CommandArgs!.Trim());
                builder.Append(" ");
            }

            // namespace
            var ns = RootNamespace;
            if (!string.IsNullOrEmpty(itemNamespace))
                ns = $"{ns}.{itemNamespace}";

            builder.Append($"-n {ns}");
            builder.Append(" ");

            // input
            builder.Append($"-f {inputFilePath}");
            builder.Append(" ");

            // output
            builder.Append($"-o {outputFilePath}");

            return builder.ToString();
        }
    }
}
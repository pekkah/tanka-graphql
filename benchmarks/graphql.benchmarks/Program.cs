using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

namespace Tanka.GraphQL.Benchmarks;

public class Program
{
    private static void Main(string[] args)
    {
        var runner = BenchmarkSwitcher
            .FromAssembly(typeof(Program).Assembly);

        if (args.Length == 0)
            runner.RunAll(GetGlobalConfig());
        else
            runner.Run(args, GetGlobalConfig());
    }

    private static IConfig GetGlobalConfig()
    {
        return DefaultConfig.Instance
            .WithArtifactsPath("artifacts/benchmarks")
            .AddJob(Job.Default
                .AsDefault());
    }
}
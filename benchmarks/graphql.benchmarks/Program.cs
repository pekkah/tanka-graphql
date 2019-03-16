using BenchmarkDotNet.Running;

namespace tanka.graphql.benchmarks
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var runner = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly);

            if (args.Length == 0)
                runner.RunAll();
            else
            {
                runner.Run(args);
            }

        }
    }
}
using BenchmarkDotNet.Running;

namespace fugu.graphql.benchmarks
{
    public class Program
    {
        private static void Main(string[] args)
        {
            BenchmarkRunner.Run<Benchmarks>();
        }
    }
}
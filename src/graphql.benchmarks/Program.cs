using BenchmarkDotNet.Running;

namespace tanka.graphql.benchmarks
{
    public class Program
    {
        private static void Main(string[] args)
        {
            BenchmarkRunner.Run<Benchmarks>();
        }
    }
}
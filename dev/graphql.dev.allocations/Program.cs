using Tanka.GraphQL.Benchmarks.Experimental;

var bench = new ExecutionBenchmarks();
bench.Setup();
await bench.Query_Complex_with_validation();
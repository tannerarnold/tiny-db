// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Running;
using TinyDb.Benchmarks;

BenchmarkRunner.Run<TinyQueueBenchmarks>();
BenchmarkRunner.Run<TinyRowListBenchmarks>();
BenchmarkRunner.Run<DatabaseBenchmarks>();


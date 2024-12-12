using BenchmarkDotNet.Attributes;
using TinyDb.Library;

namespace TinyDb.Benchmarks;

public class TinyQueueBenchmarks
{

    public TinyQueue<int> Queue { get; set; } = new TinyQueue<int>();

    [Params(100, 1000, 10000)]
    public int QueueLength { get; set; }

    [IterationSetup]
    public void Setup()
    {
        Queue = new TinyQueue<int>();
        for (int i = 0; i < QueueLength; i++)
        {
            Queue.Enqueue(Random.Shared.Next());
        }
    }

    [IterationCleanup]
    public void Deconstruct()
    {
        Queue = new TinyQueue<int>();
    }

    [Benchmark]
    public void Enqueue() => Queue.Enqueue(Random.Shared.Next());

    [Benchmark]
    public int Dequeue() => Queue.Dequeue();

}
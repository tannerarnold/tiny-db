using TinyDb.Library;
using Xunit;

namespace TinyDb.Specs;

public class TinyQueueTests
{
    readonly TinyQueue<int> _queue;

    public TinyQueueTests()
    {
        _queue = new TinyQueue<int>();
        Assert.True(_queue.IsEmpty);
    }

    [Fact]
    public void EnqueueSingleValue_Should_AddToQueue()
    {
        int val = 32;
        _queue.Enqueue(val);

        Assert.False(_queue.IsEmpty);
        Assert.Equal(1, _queue.Length);
        Assert.Equal(val, _queue.Peek());
    }

    [Fact]
    public void EnqueueMultipleValues_Should_AddToQueueAndShowFirstEnteredValue()
    {
        int[] vals = [23, 32, 45, 54];
        for (int i = 0; i < vals.Length; i++)
        {
            _queue.Enqueue(vals[i]);
        }

        Assert.False(_queue.IsEmpty);
        Assert.Equal(vals.Length, _queue.Length);
        Assert.Equal(vals[0], _queue.Peek());
    }

    [Fact]
    public void Peek_Should_ThrowWhenQueueIsEmpty()
    {
        Assert.Equal(0, _queue.Length);
        Assert.Throws<InvalidOperationException>(() => _queue.Peek());
    }

    [Fact]
    public void Dequeue_Should_RemoveFromQueue()
    {
        int enqueuedVal = 32;
        _queue.Enqueue(enqueuedVal);

        Assert.False(_queue.IsEmpty);
        Assert.Equal(1, _queue.Length);
        Assert.Equal(enqueuedVal, _queue.Peek());

        int dequeuedVal = _queue.Dequeue();

        Assert.True(_queue.IsEmpty);
        Assert.Equal(0, _queue.Length);
        Assert.Equal(enqueuedVal, dequeuedVal);
        Assert.Throws<InvalidOperationException>(() => _queue.Peek());
    }

    [Fact]
    public void Dequeue_Should_RemoveOnlyFirstValueFromQueue()
    {
        int[] enqueuedVals = [23, 32, 45, 54];
        for (int i = 0; i < enqueuedVals.Length; i++)
        {
            _queue.Enqueue(enqueuedVals[i]);
        }

        Assert.False(_queue.IsEmpty);
        Assert.Equal(enqueuedVals.Length, _queue.Length);
        Assert.Equal(enqueuedVals[0], _queue.Peek());

        int dequeuedVal = _queue.Dequeue();

        Assert.False(_queue.IsEmpty);
        Assert.Equal(enqueuedVals.Length - 1, _queue.Length);
        Assert.Equal(enqueuedVals[0], dequeuedVal);
        Assert.Equal(enqueuedVals[1], _queue.Peek());
    }

    [Fact]
    public void Dequeue_Should_ThrowWhenQueueIsEmpty()
    {
        Assert.Equal(0, _queue.Length);
        Assert.Throws<InvalidOperationException>(() => _queue.Dequeue());
    }
}
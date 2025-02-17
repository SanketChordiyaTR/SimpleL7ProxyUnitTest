using System.Collections.Concurrent;

public class PriorityQueueItem<T>
{
    public T Item { get; }
    public int Priority { get; }
    public DateTime Timestamp { get; }

    public PriorityQueueItem(T item, int priority)
    {
        Item = item;
        Priority = priority;
        Timestamp = DateTime.UtcNow;
    }
}
public class PriorityQueue<T>
{
    private readonly List<PriorityQueueItem<T>> _items = new List<PriorityQueueItem<T>>();
    private static readonly PriorityQueueItemComparer<T> Comparer = new PriorityQueueItemComparer<T>();

    public int Count => _items.Count;

    public void Enqueue(T item, int priority)
    {
        var queueItem = new PriorityQueueItem<T>(item, priority);

        // inserting into a sorted list is best with binary search:  O(n)
        int index = _items.BinarySearch(queueItem, Comparer);
        if (index < 0) index = ~index; // If not found, BinarySearch returns the bitwise complement of the index of the next element that is larger.
        _items.Insert(index, queueItem);

        //Console.WriteLine($"Enqueue:  Priority: {priority}  length: {_items.Count}  index: {index} : {GetItemsAsCommaSeparatedString()}");
    }

    public string GetItemsAsCommaSeparatedString()
    {
        return string.Join(", ", _items.Select(i => $"{i.Priority} "));
    }

    public T Dequeue()
    {
        if (_items.Count == 0)
            throw new InvalidOperationException("The queue is empty.");

        // var item = _items[_items.Count - 1]; // Get the last item
        // _items.RemoveAt(_items.Count - 1); // Remove the last item
        var item = _items[0]; // Get the first item
        _items.RemoveAt(0); // Remove the first item

        return item.Item;
    }
}

public class PriorityQueueItemComparer<T> : IComparer<PriorityQueueItem<T>>
{
    public int Compare(PriorityQueueItem<T>? x, PriorityQueueItem<T>? y)
    {
        if (x == null)
        {
            return y == null ? 0 : -1; // If x is null and y is not, x is considered smaller
        }
        if (y == null)
        {
            return 1; // If y is null and x is not, x is considered larger
        }

        int priorityComparison = x.Priority.CompareTo(y.Priority);
        if (priorityComparison == 0)
        {
            // If priorities are equal, sort by timestamp (older items are "bigger")
            //return y.Timestamp.CompareTo(x.Timestamp);
            return x.Timestamp.CompareTo(y.Timestamp);
        }
        return priorityComparison;
    }
}

public class BlockingPriorityQueue<T>
{
    private readonly PriorityQueue<T> _priorityQueue = new PriorityQueue<T>();
    private readonly object _lock = new object();

    public int MaxQueueLength { get; set; }

    public int Count => _priorityQueue.Count;

    public bool Enqueue(T item, int priority)
    {
        lock (_lock)
        {
            //Console.WriteLine($"Count: {_priorityQueue.Count} , Max: {MaxQueueLength}");
            if (_priorityQueue.Count >= MaxQueueLength)
            {
                return false;
            }
            _priorityQueue.Enqueue(item, priority);
            Monitor.Pulse(_lock); // Signal that an item has been added
        }

        return true;
    }

    public T Dequeue(CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            while (_priorityQueue.Count == 0)
            {
                Monitor.Wait(_lock, 500); // Wait for an item to be added
                cancellationToken.ThrowIfCancellationRequested();
            }
            return _priorityQueue.Dequeue();
        }
    }
    
}


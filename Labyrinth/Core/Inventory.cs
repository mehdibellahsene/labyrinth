namespace Labyrinth.Core;

using System.Collections.Concurrent;
using Labyrinth.Items;

public class AsyncInventory
{
    private readonly ConcurrentBag<ICollectable> _items = new();
    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task<IEnumerable<ICollectable>> ListItemsAsync(CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            return _items.ToArray();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<bool> TryTakeItemAsync(ICollectable item, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            var items = _items.ToList();
            if (!items.Contains(item)) return false;

            _items.Clear();
            foreach (var i in items.Where(i => !i.Equals(item)))
                _items.Add(i);
            return true;
        }
        finally
        {
            _lock.Release();
        }
    }

    public void Add(ICollectable item) => _items.Add(item);
}

namespace Labyrinth.Items;

public class LocalInventory : Inventory
{
    private readonly object _lock = new();

    protected LocalInventory(ICollectable? item = null) : base(item) { }

    public void MoveFirst(LocalInventory from)
    {
        if (!TryMoveItemsFromSync(from, from.ItemTypes.Select((_, i) => i == 0).ToList()))
            throw new ArgumentException("Specified source inventory may be empty.");
    }

    public async Task MoveFirstAsync(LocalInventory from)
    {
        if (!await TryMoveItemsFrom(from, from.ItemTypes.Select((_, i) => i == 0).ToList()))
            throw new ArgumentException("Specified source inventory may be empty.");
    }

    public bool TryMoveItemsFromSync(LocalInventory from, IList<bool> movesRequired)
    {
        lock (_lock) lock (from._lock)
        {
            if (movesRequired.Count != from._items.Count) return false;
            for (int i = movesRequired.Count - 1; i >= 0; i--)
                if (movesRequired[i]) { _items.Add(from._items[i]); from._items.RemoveAt(i); }
            return true;
        }
    }

    public override Task<bool> TryMoveItemsFrom(Inventory from, IList<bool> movesRequired) =>
        from is LocalInventory other
            ? Task.FromResult(TryMoveItemsFromSync(other, movesRequired))
            : throw new ArgumentException("Source inventory must be of type LocalInventory.", nameof(from));
}

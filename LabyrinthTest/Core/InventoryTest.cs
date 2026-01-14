namespace LabyrinthTest.Core;

using Labyrinth.Core;
using Labyrinth.Items;

[TestFixture]
public class InventoryTest
{
    [Test]
    public async Task ListItems_Empty_ReturnsEmpty()
    {
        var inventory = new AsyncInventory();

        var items = await inventory.ListItemsAsync();

        Assert.That(items, Is.Empty);
    }

    [Test]
    public async Task ListItems_WithItems_ReturnsAll()
    {
        var inventory = new AsyncInventory();
        var key = new Key();
        inventory.Add(key);

        var items = await inventory.ListItemsAsync();

        Assert.That(items.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task TryTakeItem_Exists_ReturnsTrue()
    {
        var inventory = new AsyncInventory();
        var key = new Key();
        inventory.Add(key);

        var result = await inventory.TryTakeItemAsync(key);

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task TryTakeItem_NotExists_ReturnsFalse()
    {
        var inventory = new AsyncInventory();
        var key = new Key();

        var result = await inventory.TryTakeItemAsync(key);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task TryTakeItem_RemovesFromInventory()
    {
        var inventory = new AsyncInventory();
        var key = new Key();
        inventory.Add(key);

        await inventory.TryTakeItemAsync(key);
        var items = await inventory.ListItemsAsync();

        Assert.That(items, Is.Empty);
    }

    [Test]
    public async Task ListItems_WithCancellation_ThrowsOperationCanceledException()
    {
        var inventory = new AsyncInventory();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await inventory.ListItemsAsync(cts.Token));
    }
}

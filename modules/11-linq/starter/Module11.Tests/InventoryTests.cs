using System.Collections;
using Module11.Tasks;

namespace Module11.Tests;

public class InventoryTests
{
    private static Inventory CreateInventory()
    {
        var inventory = new Inventory();
        inventory.Add(new StockItem("A-1", "Tools", 2));
        inventory.Add(new StockItem("B-2", "Tools", 50));
        inventory.Add(new StockItem("C-3", "Paint", 1));
        return inventory;
    }

    [Fact]
    public void GetLowStockSnapshot_ReturnsItemsBelowThreshold()
    {
        var inventory = CreateInventory();

        var low = inventory.GetLowStockSnapshot(5);

        Assert.Equal(new[] { "A-1", "C-3" }, low.Select(i => i.Sku));
    }

    [Fact]
    public void GetLowStockSnapshot_IsNotAffectedByLaterChanges()
    {
        var inventory = CreateInventory();
        var snapshot = inventory.GetLowStockSnapshot(5);

        inventory.Add(new StockItem("D-4", "Paint", 0));

        Assert.Equal(2, snapshot.Count());
    }

    [Fact]
    public void GetLowStockSnapshot_CanBeEnumeratedWhileInventoryChanges()
    {
        var inventory = CreateInventory();
        var snapshot = inventory.GetLowStockSnapshot(5);

        // Перебираем снимок и одновременно меняем склад — со «живым» запросом это InvalidOperationException.
        foreach (StockItem item in snapshot)
        {
            inventory.Add(item with { Sku = item.Sku + "-copy" });
        }

        Assert.Equal(5, inventory.Count);
    }

    [Fact]
    public void ImportSkus_NormalizesAndDeduplicates()
    {
        var inventory = new Inventory();

        var report = inventory.ImportSkus(new[] { " a-1", "A-1 ", "b-2", "c-3", "B-2" });

        Assert.Equal(5, report.TotalCount);
        Assert.Equal(new[] { "A-1", "B-2", "C-3" }, report.UniqueSkus);
    }

    [Fact]
    public void ImportSkus_NormalizesEachSkuExactlyOnce()
    {
        var inventory = new Inventory();

        inventory.ImportSkus(new[] { "a", "b", "a" });

        Assert.Equal(3, inventory.NormalizeCalls);
    }

    [Fact]
    public void ImportSkus_EnumeratesSourceOnlyOnce()
    {
        var inventory = new Inventory();
        var source = new OneShotSequence<string>(new[] { "x-1", "x-2" });

        var report = inventory.ImportSkus(source);

        Assert.Equal(2, report.TotalCount);
    }

    [Fact]
    public void RemoveEmpty_RemovesZeroQuantityItems_AndReturnsCount()
    {
        var inventory = CreateInventory();
        inventory.Add(new StockItem("Z-1", "Tools", 0));
        inventory.Add(new StockItem("Z-2", "Paint", 0));

        int removed = inventory.RemoveEmpty();

        Assert.Equal(2, removed);
        Assert.Equal(3, inventory.Count);
    }

    [Fact]
    public void RemoveEmpty_NothingToRemove_ReturnsZero()
    {
        var inventory = CreateInventory();

        Assert.Equal(0, inventory.RemoveEmpty());
        Assert.Equal(3, inventory.Count);
    }

    /// <summary>
    /// Последовательность, которую можно перечислить только один раз — как поток из файла,
    /// сетевой ответ или DataReader. Второй GetEnumerator бросает исключение.
    /// </summary>
    private sealed class OneShotSequence<T> : IEnumerable<T>
    {
        private readonly IEnumerable<T> _items;
        private bool _enumerated;

        public OneShotSequence(IEnumerable<T> items) => _items = items;

        public IEnumerator<T> GetEnumerator()
        {
            if (_enumerated)
            {
                throw new InvalidOperationException("This sequence can be enumerated only once.");
            }

            _enumerated = true;
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

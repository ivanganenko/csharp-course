using Module06.Tasks;

namespace Module06.Tests;

public class WarehouseTests
{
    [Fact]
    public void Receive_NewSku_AddsEntry()
    {
        var warehouse = new Warehouse();

        warehouse.Receive("APL-1", 10);

        Assert.Single(warehouse.Entries);
        Assert.Equal("APL-1", warehouse.Entries[0].Sku);
        Assert.Equal(10, warehouse.GetQuantity("APL-1"));
    }

    [Fact]
    public void Receive_ExistingSku_IncreasesQuantityInList()
    {
        // Наивная реализация через _entries[i].Add(...) или foreach меняет КОПИЮ — этот тест её ловит.
        var warehouse = new Warehouse();

        warehouse.Receive("APL-1", 10);
        warehouse.Receive("APL-1", 5);

        Assert.Single(warehouse.Entries);
        Assert.Equal(15, warehouse.Entries[0].Quantity);
        Assert.Equal(15, warehouse.GetQuantity("APL-1"));
    }

    [Fact]
    public void Receive_DifferentSkus_KeepsOrder()
    {
        var warehouse = new Warehouse();

        warehouse.Receive("B", 1);
        warehouse.Receive("A", 2);
        warehouse.Receive("B", 3);

        Assert.Equal("B:4 A:2", string.Join(" ", warehouse.Entries.Select(e => $"{e.Sku}:{e.Quantity}")));
    }

    [Fact]
    public void Receive_InvalidArguments_Throw()
    {
        var warehouse = new Warehouse();

        Assert.ThrowsAny<ArgumentException>(() => warehouse.Receive(" ", 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => warehouse.Receive("A", 0));
        Assert.Empty(warehouse.Entries);
    }

    [Fact]
    public void TryShip_EnoughStock_DecreasesQuantity()
    {
        var warehouse = new Warehouse();
        warehouse.Receive("APL-1", 10);

        Assert.True(warehouse.TryShip("APL-1", 4));
        Assert.Equal(6, warehouse.GetQuantity("APL-1"));
        Assert.True(warehouse.TryShip("APL-1", 6));
        Assert.Equal(0, warehouse.GetQuantity("APL-1"));
        Assert.Single(warehouse.Entries);
    }

    [Fact]
    public void TryShip_NotEnoughOrUnknown_ReturnsFalseAndChangesNothing()
    {
        var warehouse = new Warehouse();
        warehouse.Receive("APL-1", 3);

        Assert.False(warehouse.TryShip("APL-1", 4));
        Assert.False(warehouse.TryShip("apl-1", 1));
        Assert.False(warehouse.TryShip("NOPE", 1));
        Assert.Equal(3, warehouse.GetQuantity("APL-1"));
        Assert.Throws<ArgumentOutOfRangeException>(() => warehouse.TryShip("APL-1", 0));
    }

    [Fact]
    public void GetQuantity_UnknownSku_IsZero()
    {
        Assert.Equal(0, new Warehouse().GetQuantity("X"));
    }

    [Fact]
    public void Entries_ReturnsCopiesOfStructs()
    {
        var warehouse = new Warehouse();
        warehouse.Receive("A", 1);

        var copy = warehouse.Entries[0];
        copy.Add(100);           // меняем локальную копию...

        Assert.Equal(101, copy.Quantity);
        Assert.Equal(1, warehouse.GetQuantity("A")); // ...склад это не затрагивает
    }
}

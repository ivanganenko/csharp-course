using Module02.Tasks;

namespace Module02.Tests;

public class ReceiptBuilderTests
{
    private static readonly DateOnly Date = new(2026, 3, 15);

    [Fact]
    public void BuildReceipt_TwoLines_ReturnsFormattedReceipt()
    {
        ReceiptLine[] lines =
        [
            new("Tea", 2.5m, 2),
            new("Coffee", 3.5m, 1),
        ];

        var receipt = ReceiptBuilder.BuildReceipt(42, Date, lines);

        Assert.Equal(
            "Receipt #42 (2026-03-15)\n" +
            "Tea x2 = 5.00\n" +
            "Coffee x1 = 3.50\n" +
            "Total: 8.50\n",
            receipt);
    }

    [Fact]
    public void BuildReceipt_NoLines_ReturnsHeaderAndZeroTotal()
    {
        var receipt = ReceiptBuilder.BuildReceipt(1, Date, []);

        Assert.Equal("Receipt #1 (2026-03-15)\nTotal: 0.00\n", receipt);
    }

    [Fact]
    public void BuildReceipt_AlwaysUsesDotAndTwoDecimals()
    {
        ReceiptLine[] lines = [new("Bread", 1.2m, 3), new("Milk", 0.99m, 1)];

        var receipt = ReceiptBuilder.BuildReceipt(7, new DateOnly(2026, 1, 5), lines);

        Assert.Equal(
            "Receipt #7 (2026-01-05)\n" +
            "Bread x3 = 3.60\n" +
            "Milk x1 = 0.99\n" +
            "Total: 4.59\n",
            receipt);
    }

    [Fact]
    public void BuildReceipt_ManyLines_TotalIsExact()
    {
        // 1000 строк по 0.10 — на double сумма «поплыла» бы, на decimal ровно 100.00.
        var lines = new ReceiptLine[1000];
        for (var i = 0; i < lines.Length; i++)
        {
            lines[i] = new ReceiptLine("Item", 0.1m, 1);
        }

        var receipt = ReceiptBuilder.BuildReceipt(2, Date, lines);

        Assert.EndsWith("Total: 100.00\n", receipt);
        Assert.DoesNotContain("\r", receipt);
    }

    [Fact]
    public void BuildReceipt_NullLines_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => ReceiptBuilder.BuildReceipt(1, Date, null!));
    }

    [Fact]
    public void BuildReceipt_ZeroQuantity_Throws()
    {
        ReceiptLine[] lines = [new("Tea", 2.5m, 0)];

        Assert.Throws<ArgumentException>(() => ReceiptBuilder.BuildReceipt(1, Date, lines));
    }

    [Fact]
    public void BuildReceipt_NegativePrice_Throws()
    {
        ReceiptLine[] lines = [new("Tea", -2.5m, 1)];

        Assert.Throws<ArgumentException>(() => ReceiptBuilder.BuildReceipt(1, Date, lines));
    }
}

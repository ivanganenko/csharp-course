using Module09.Tasks;

namespace Module09.Tests;

public class DeduplicatorTests
{
    [Fact]
    public void RemoveDuplicates_Ints_KeepsFirstOccurrencesInOrder()
    {
        var result = Deduplicator.RemoveDuplicates(new[] { 3, 1, 3, 2, 1, 3 });

        Assert.Equal(new[] { 3, 1, 2 }, result);
    }

    [Fact]
    public void RemoveDuplicates_WithIgnoreCaseComparer_KeepsFirstSpelling()
    {
        var emails = new[] { "Anna@Example.com", "bob@example.com", "anna@example.com", "BOB@EXAMPLE.COM" };

        var result = Deduplicator.RemoveDuplicates(emails, StringComparer.OrdinalIgnoreCase);

        Assert.Equal(new[] { "Anna@Example.com", "bob@example.com" }, result);
    }

    [Fact]
    public void RemoveDuplicates_DefaultComparer_IsCaseSensitive()
    {
        var result = Deduplicator.RemoveDuplicates(new[] { "a", "A", "a" });

        Assert.Equal(new[] { "a", "A" }, result);
    }

    [Fact]
    public void RemoveDuplicates_Empty_ReturnsEmpty()
    {
        Assert.Empty(Deduplicator.RemoveDuplicates(Array.Empty<int>()));
    }

    [Fact]
    public void RemoveDuplicates_HandlesNullElements()
    {
        var result = Deduplicator.RemoveDuplicates(new string?[] { null, "x", null });

        Assert.Equal(new[] { null, "x" }, result);
    }

    [Fact]
    public void RemoveDuplicates_Null_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Deduplicator.RemoveDuplicates<int>(null!));
    }

    [Fact]
    public void RemoveDuplicates_LargeInput_ContainsEachValueOnce()
    {
        // 200 000 элементов: решение O(n²) с List.Contains заметно «задумается» — повод обсудить сложность.
        var input = Enumerable.Range(0, 200_000).Select(i => i % 1000);

        var result = Deduplicator.RemoveDuplicates(input);

        Assert.Equal(Enumerable.Range(0, 1000), result);
    }

    [Fact]
    public void FindDuplicates_ReturnsEachRepeatedOnce_InOrderOfSecondOccurrence()
    {
        var result = Deduplicator.FindDuplicates(new[] { 3, 1, 3, 2, 1, 3 });

        Assert.Equal(new[] { 3, 1 }, result);
    }

    [Fact]
    public void FindDuplicates_OrderIsBySecondOccurrence_NotFirst()
    {
        // 1 появляется первым, но повторяется позже, чем 2.
        var result = Deduplicator.FindDuplicates(new[] { 1, 2, 2, 1 });

        Assert.Equal(new[] { 2, 1 }, result);
    }

    [Fact]
    public void FindDuplicates_NoDuplicates_ReturnsEmpty()
    {
        Assert.Empty(Deduplicator.FindDuplicates(new[] { 1, 2, 3 }));
    }

    [Fact]
    public void FindDuplicates_WithComparer_UsesIt()
    {
        var result = Deduplicator.FindDuplicates(new[] { "SKU-1", "sku-1", "SKU-2" }, StringComparer.OrdinalIgnoreCase);

        // Какое из написаний попадёт в результат — не важно, важно, что дубликат найден один.
        Assert.Single(result);
        Assert.Equal("SKU-1", result[0], ignoreCase: true);
    }
}

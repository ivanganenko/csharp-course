using Module08.Tasks;

namespace Module08.Tests;

public class SimpleStackTests
{
    [Fact]
    public void NewStack_IsEmpty()
    {
        var stack = new SimpleStack<int>();

        Assert.True(stack.IsEmpty);
        Assert.Equal(0, stack.Count);
    }

    [Fact]
    public void Push_ThenPop_ReturnsItemsInLifoOrder()
    {
        var stack = new SimpleStack<string>();
        stack.Push("first");
        stack.Push("second");
        stack.Push("third");

        Assert.Equal(3, stack.Count);
        Assert.Equal("third", stack.Pop());
        Assert.Equal("second", stack.Pop());
        Assert.Equal("first", stack.Pop());
        Assert.True(stack.IsEmpty);
    }

    [Fact]
    public void Peek_ReturnsTopWithoutRemoving()
    {
        var stack = new SimpleStack<int>();
        stack.Push(1);
        stack.Push(2);

        Assert.Equal(2, stack.Peek());
        Assert.Equal(2, stack.Peek());
        Assert.Equal(2, stack.Count);
    }

    [Fact]
    public void Pop_OnEmptyStack_ThrowsInvalidOperationException()
    {
        var stack = new SimpleStack<int>();

        Assert.Throws<InvalidOperationException>(() => stack.Pop());
    }

    [Fact]
    public void Peek_OnEmptyStack_ThrowsInvalidOperationException()
    {
        var stack = new SimpleStack<string>();

        Assert.Throws<InvalidOperationException>(() => stack.Peek());
    }

    [Fact]
    public void TryPop_OnEmptyStack_ReturnsFalseAndDefault()
    {
        var stack = new SimpleStack<int>();

        bool success = stack.TryPop(out int item);

        Assert.False(success);
        Assert.Equal(0, item);
    }

    [Fact]
    public void TryPop_OnNonEmptyStack_ReturnsTopItem()
    {
        var stack = new SimpleStack<int>();
        stack.Push(10);
        stack.Push(20);

        bool success = stack.TryPop(out int item);

        Assert.True(success);
        Assert.Equal(20, item);
        Assert.Equal(1, stack.Count);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(4)]
    public void Push_MoreThanInitialCapacity_GrowsAndKeepsOrder(int initialCapacity)
    {
        var stack = new SimpleStack<int>(initialCapacity);

        for (int i = 0; i < 100; i++)
        {
            stack.Push(i);
        }

        Assert.Equal(100, stack.Count);
        for (int expected = 99; expected >= 0; expected--)
        {
            Assert.Equal(expected, stack.Pop());
        }
    }

    [Fact]
    public void Constructor_NegativeCapacity_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new SimpleStack<int>(-1));
    }

    [Fact]
    public void Enumerate_ReturnsItemsFromTopToBottom_WithoutChangingStack()
    {
        var stack = new SimpleStack<char>();
        stack.Push('a');
        stack.Push('b');
        stack.Push('c');

        var items = new List<char>();
        foreach (char c in stack)
        {
            items.Add(c);
        }

        Assert.Equal(new[] { 'c', 'b', 'a' }, items);
        Assert.Equal(3, stack.Count);
    }

    [Fact]
    public void Clear_RemovesAllItems_AndStackIsReusable()
    {
        var stack = new SimpleStack<int>();
        stack.Push(1);
        stack.Push(2);

        stack.Clear();

        Assert.True(stack.IsEmpty);
        Assert.Throws<InvalidOperationException>(() => stack.Pop());
        stack.Push(42);
        Assert.Equal(42, stack.Peek());
    }

    [Fact]
    public void Push_NullForReferenceType_IsAllowed()
    {
        var stack = new SimpleStack<string?>();
        stack.Push("text");
        stack.Push(null);

        Assert.Null(stack.Pop());
        Assert.Equal("text", stack.Pop());
    }
}

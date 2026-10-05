using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Module08.Tasks;

public class SimpleStack<T> : IEnumerable<T>
{
    private const int DefaultCapacity = 4;

    // Массив T[] — а не object[]: для SimpleStack<int> элементы хранятся без boxing,
    // ради этого generics и придуманы.
    private T[] _items;
    private int _count;

    // Цепочка конструкторов: вся валидация — в одном месте.
    public SimpleStack() : this(DefaultCapacity)
    {
    }

    public SimpleStack(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
        _items = new T[capacity];
    }

    public int Count => _count;

    public bool IsEmpty => _count == 0;

    public void Push(T item)
    {
        if (_count == _items.Length)
        {
            Grow();
        }

        _items[_count] = item;
        _count++;
    }

    public T Pop()
    {
        ThrowIfEmpty();

        _count--;
        T item = _items[_count];
        // Почему обнуляем ячейку: иначе массив продолжит ссылаться на объект,
        // и GC не сможет его собрать, хотя логически он из стека уже удалён («утечка» через коллекцию).
        _items[_count] = default!;
        return item;
    }

    public T Peek()
    {
        ThrowIfEmpty();
        return _items[_count - 1];
    }

    public bool TryPop([MaybeNullWhen(false)] out T item)
    {
        // Try-pattern: «пустой стек» здесь — ожидаемая ситуация, а не ошибка, поэтому без исключений
        // (исключения дорогие и предназначены для исключительных случаев).
        if (_count == 0)
        {
            item = default;
            return false;
        }

        item = Pop();
        return true;
    }

    public void Clear()
    {
        // Array.Clear тоже нужен ради GC: отпускаем ссылки на все объекты.
        Array.Clear(_items, 0, _count);
        _count = 0;
    }

    public IEnumerator<T> GetEnumerator()
    {
        // yield return: компилятор сам генерирует класс-итератор (state machine).
        for (int i = _count - 1; i >= 0; i--)
        {
            yield return _items[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private void Grow()
    {
        // Удвоение ёмкости даёт амортизированную O(1) на Push: копирования случаются всё реже.
        // Рост на +1 дал бы O(n) на каждый Push и O(n^2) в сумме. Так же растёт List<T>.
        int newCapacity = _items.Length == 0 ? DefaultCapacity : _items.Length * 2;
        Array.Resize(ref _items, newCapacity);
    }

    private void ThrowIfEmpty()
    {
        if (_count == 0)
        {
            // InvalidOperationException — стандарт BCL для «операция недопустима в текущем состоянии объекта»
            // (так же ведёт себя Stack<T>.Pop).
            throw new InvalidOperationException("Stack is empty.");
        }
    }
}

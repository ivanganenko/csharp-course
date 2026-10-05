using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Module08.Tasks;

/// <summary>
/// Задача 08.1 — собственный generic-стек (LIFO: last in, first out) на основе массива.
/// Не используйте внутри <see cref="Stack{T}"/> или <see cref="List{T}"/> — только массив <c>T[]</c>.
/// Тесты: SimpleStackTests.
/// </summary>
public class SimpleStack<T> : IEnumerable<T>
{
    /// <summary>
    /// Создаёт пустой стек с начальной ёмкостью (capacity) по умолчанию — 4 элемента.
    /// </summary>
    public SimpleStack()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Создаёт пустой стек с заданной начальной ёмкостью.
    /// capacity = 0 допустима (массив вырастет при первом Push).
    /// Отрицательная ёмкость → <see cref="ArgumentOutOfRangeException"/>.
    /// </summary>
    public SimpleStack(int capacity)
    {
        throw new NotImplementedException();
    }

    /// <summary>Количество элементов в стеке.</summary>
    public int Count => throw new NotImplementedException();

    /// <summary>true, если стек пуст.</summary>
    public bool IsEmpty => throw new NotImplementedException();

    /// <summary>
    /// Кладёт элемент на вершину стека. Если массив заполнен — увеличьте его (обычно вдвое)
    /// и скопируйте элементы. null для reference types — допустимое значение.
    /// </summary>
    public void Push(T item)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Снимает и возвращает элемент с вершины.
    /// Пустой стек → <see cref="InvalidOperationException"/>.
    /// Освободите ячейку массива (запишите туда default), чтобы не держать ссылку на объект.
    /// </summary>
    public T Pop()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Возвращает элемент с вершины, НЕ снимая его.
    /// Пустой стек → <see cref="InvalidOperationException"/>.
    /// </summary>
    public T Peek()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Try-pattern: если стек не пуст — снимает элемент, кладёт его в <paramref name="item"/> и возвращает true.
    /// Если пуст — возвращает false, а <paramref name="item"/> = default(T). Исключений не бросает.
    /// </summary>
    public bool TryPop([MaybeNullWhen(false)] out T item)
    {
        throw new NotImplementedException();
    }

    /// <summary>Удаляет все элементы. После Clear стек пуст и снова пригоден к работе.</summary>
    public void Clear()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Перебирает элементы от вершины ко дну (в том порядке, в каком их вернул бы Pop),
    /// не изменяя стек. Подсказка: yield return.
    /// </summary>
    public IEnumerator<T> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    // Необобщённый IEnumerable требуется интерфейсом IEnumerable<T> (наследие .NET 1.0, когда generics не было).
    // Просто делегируем в generic-версию — здесь ничего менять не нужно.
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

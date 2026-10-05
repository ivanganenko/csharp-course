namespace Module08.Tasks;

/// <summary>
/// Задача 08.2 — generic-методы и constraints.
/// Не используйте LINQ (Max/Count и т.п.) — смысл задачи в том, чтобы написать алгоритм самому.
/// Тесты: GenericAlgorithmsTests.
/// </summary>
public static class GenericAlgorithms
{
    /// <summary>
    /// Возвращает максимальный элемент последовательности, сравнивая через <see cref="IComparable{T}.CompareTo"/>.
    /// null вместо последовательности → <see cref="ArgumentNullException"/>.
    /// Пустая последовательность → <see cref="InvalidOperationException"/>.
    /// Последовательность перебирается ровно один раз.
    /// </summary>
    public static T Max<T>(IEnumerable<T> items) where T : IComparable<T>
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// «Зажимает» значение в диапазон [min; max]: меньше min → min, больше max → max, иначе само значение.
    /// Если min больше max → <see cref="ArgumentException"/>.
    /// </summary>
    public static T Clamp<T>(T value, T min, T max) where T : IComparable<T>
    {
        throw new NotImplementedException();
    }

    /// <summary>Меняет местами значения двух переменных любого типа.</summary>
    public static void Swap<T>(ref T first, ref T second)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Считает, сколько элементов равны default(T): 0 для int, null для string, false для bool и т.д.
    /// Сравнение — через <see cref="EqualityComparer{T}.Default"/> (почему не == — см. README, раздел про default).
    /// null вместо последовательности → <see cref="ArgumentNullException"/>.
    /// </summary>
    public static int CountDefaults<T>(IEnumerable<T> items)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Создаёт список из <paramref name="count"/> НОВЫХ экземпляров T (каждый — отдельный объект).
    /// count = 0 → пустой список. Отрицательный count → <see cref="ArgumentOutOfRangeException"/>.
    /// Подсказка: какой constraint позволяет написать <c>new T()</c>?
    /// </summary>
    public static List<T> CreateMany<T>(int count) where T : new()
    {
        throw new NotImplementedException();
    }
}

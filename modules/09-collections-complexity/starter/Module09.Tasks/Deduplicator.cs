namespace Module09.Tasks;

/// <summary>
/// Задача 09.2 — удаление дубликатов с сохранением порядка (например, список email-рассылки
/// или история просмотренных товаров). LINQ (Distinct, GroupBy) не используйте.
/// Тесты: DeduplicatorTests.
/// </summary>
public static class Deduplicator
{
    /// <summary>
    /// Возвращает новый список без повторов, сохраняя порядок ПЕРВЫХ вхождений.
    /// <paramref name="comparer"/> задаёт правило равенства (например, <see cref="StringComparer.OrdinalIgnoreCase"/>);
    /// null → сравнение по умолчанию (<see cref="EqualityComparer{T}.Default"/>).
    /// null вместо <paramref name="items"/> → <see cref="ArgumentNullException"/>.
    /// Сложность — O(n): никаких вложенных циклов и <c>List.Contains</c> внутри цикла.
    /// </summary>
    public static List<T> RemoveDuplicates<T>(IEnumerable<T> items, IEqualityComparer<T>? comparer = null)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Возвращает элементы, которые встречаются больше одного раза. Каждый такой элемент — один раз,
    /// в порядке появления его ВТОРОГО вхождения.
    /// Пример: [3, 1, 3, 2, 1, 3] → [3, 1].
    /// null → <see cref="ArgumentNullException"/>. Сложность — O(n).
    /// </summary>
    public static List<T> FindDuplicates<T>(IEnumerable<T> items, IEqualityComparer<T>? comparer = null)
    {
        throw new NotImplementedException();
    }
}

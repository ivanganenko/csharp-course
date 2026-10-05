namespace Module13.Tasks;

/// <summary>
/// Задача 13.6 — асинхронный поток данных: IAsyncEnumerable&lt;T&gt;.
/// API отдаёт данные страницами; мы хотим отдавать вызывающему элементы по одному,
/// загружая следующую страницу только когда она действительно понадобилась.
/// </summary>
public static class PagedReader
{
    /// <summary>
    /// Лениво возвращает элементы страниц 0, 1, 2, … через loadPage(pageNumber, token).
    /// - Пустая страница = конец данных (дальше не загружаем).
    /// - Страница N+1 загружается только когда вызывающий запросил элемент после последнего элемента страницы N
    ///   (если вызывающий прервал await foreach после первого элемента — загружена только страница 0).
    /// - ПЕРЕД загрузкой каждой страницы проверяет отмену → OperationCanceledException.
    /// - Токен можно передать и параметром, и через .WithCancellation(token) — пометь параметр
    ///   атрибутом [EnumeratorCancellation] (namespace System.Runtime.CompilerServices).
    /// - loadPage == null → ArgumentNullException (допустимо при первой итерации).
    /// Реализуй как async-итератор: async IAsyncEnumerable&lt;string&gt; + yield return.
    /// </summary>
    public static IAsyncEnumerable<string> ReadAllAsync(
        Func<int, CancellationToken, Task<IReadOnlyList<string>>> loadPage,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}

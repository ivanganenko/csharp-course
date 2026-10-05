using System.Runtime.CompilerServices;

namespace Module13.Tasks;

public static class PagedReader
{
    // async + IAsyncEnumerable + yield return = асинхронный итератор (C# 8).
    // Компилятор строит state machine, которая умеет и await, и yield:
    // вызывающий получает элементы по одному через await foreach,
    // а мы подгружаем следующую страницу только по требованию (ленивость, как у обычного yield).
    public static async IAsyncEnumerable<string> ReadAllAsync(
        Func<int, CancellationToken, Task<IReadOnlyList<string>>> loadPage,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Важно: в итераторе эта проверка выполнится не при вызове метода,
        // а при первом MoveNextAsync (deferred execution, как в LINQ).
        ArgumentNullException.ThrowIfNull(loadPage);

        for (var page = 0; ; page++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var items = await loadPage(page, cancellationToken);
            if (items.Count == 0)
            {
                yield break;
            }

            foreach (var item in items)
            {
                yield return item;
            }
        }
    }
}

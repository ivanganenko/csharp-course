namespace Module10.Tasks;

public static class ClosureLab
{
    public static Func<int> CreateCounter()
    {
        // Локальная переменная «переживает» метод: компилятор переносит её в поле скрытого класса
        // (display class), а лямбда — метод этого класса. Каждый вызов CreateCounter создаёт
        // новый экземпляр display class — поэтому счётчики независимы.
        int count = 0;
        return () => ++count;
    }

    public static IReadOnlyList<Func<int>> CreateMultipliers(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);

        var functions = new List<Func<int>>(count);
        for (int i = 0; i < count; i++)
        {
            // Ловушка: переменная цикла for ОДНА на весь цикл. Лямбда захватывает переменную,
            // а не значение, — без копии все функции вернули бы count * 10.
            // Копия внутри тела цикла — новая переменная на каждой итерации.
            // (У foreach с C# 5 переменная своя на каждой итерации, поэтому там ловушки нет.)
            int index = i;
            functions.Add(() => index * 10);
        }

        return functions;
    }

    public static Action<string> CreateLogger(ICollection<string> sink, string prefix)
    {
        ArgumentNullException.ThrowIfNull(sink);
        ArgumentNullException.ThrowIfNull(prefix);

        // Замыкание захватывает параметры sink и prefix — «конфигурация» логгера живёт внутри делегата.
        // Это функциональная альтернатива классу с двумя полями и методом Log.
        return message => sink.Add($"[{prefix}] {message}");
    }

    public static Func<int, int> Compose(Func<int, int> first, Func<int, int> second)
    {
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);

        return x => second(first(x));
    }

    public static Func<T, TResult> Memoize<T, TResult>(Func<T, TResult> function) where T : notnull
    {
        ArgumentNullException.ThrowIfNull(function);

        // Кэш создаётся один раз на вызов Memoize и захватывается возвращаемой лямбдой:
        // у каждой мемоизированной функции — свой приватный словарь, недоступный снаружи.
        // Не потокобезопасно; для многопоточности — ConcurrentDictionary.GetOrAdd (модуль 13).
        var cache = new Dictionary<T, TResult>();
        return argument =>
        {
            if (!cache.TryGetValue(argument, out TResult? result))
            {
                result = function(argument);
                cache[argument] = result;
            }

            return result;
        };
    }
}

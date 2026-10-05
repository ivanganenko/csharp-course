namespace Module03.Tasks;

public static class PatternMatcher
{
    public static string Describe(object? value) => value switch
    {
        // Порядок важен: более конкретные паттерны — выше общих. Если поставить `int`
        // перед `0`, компилятор выдаст ошибку CS8510 (паттерн недостижим) — он нас защищает.
        null => "null",

        // constant pattern + relational pattern + type pattern
        0 => "zero",
        int and < 0 => "negative int",
        int => "positive int",

        double => "double",

        // property pattern: проверка типа и свойства одним выражением
        string { Length: 0 } => "empty string",
        string s => $"string of length {s.Length}",

        // list patterns (C# 11): [] — пусто, [var x] — ровно один, [var first, .., var last] — два и более.
        // `int[] and [...]`: сначала type pattern сужает object до int[], затем list pattern
        // (запись `int[] []` компилятор прочитал бы как тип jagged-массива int[][]).
        int[] and [] => "empty array",
        int[] and [var single] => $"single element {single}",
        int[] and [var first, .., var last] => $"array from {first} to {last}",

        // positional pattern работает, потому что у record есть Deconstruct;
        // property pattern { X: 0 } — то же самое, но явнее по именам.
        Point(0, 0) => "origin",
        Point { X: 0 } or Point { Y: 0 } => "on axis",
        Point => "point",

        DateOnly { Month: 12, Day: 31 } => "new year's eve",
        DateOnly => "date",

        // 5L — long, а не int: type pattern проверяет ТОЧНЫЙ тип упакованного значения,
        // неявных числовых преобразований при сопоставлении с образцом нет.
        _ => "unknown",
    };
}

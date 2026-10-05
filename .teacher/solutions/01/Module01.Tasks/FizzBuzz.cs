namespace Module01.Tasks;

public static class FizzBuzz
{
    public static string ForNumber(int n)
    {
        // Guard clause первым делом: некорректный вход — ошибка вызывающего кода,
        // молча вернуть "0" было бы хуже: баг всплыл бы далеко от места появления.
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(n);

        // Почему склейка, а не цепочка if/else для каждой комбинации:
        // с тремя правилами комбинаций уже 8, с четырьмя — 16. Склейка растёт линейно:
        // новое правило = одна строка. Это и есть ответ на follow-up интервьюера
        // «а если добавить правило для 11?».
        var result = "";
        if (n % 3 == 0) result += "Fizz";
        if (n % 5 == 0) result += "Buzz";
        if (n % 7 == 0) result += "Whizz";

        // Конкатенация здесь допустима: максимум три склейки коротких строк.
        // В цикле на тысячи итераций мы бы взяли StringBuilder (модуль 02).
        return result.Length == 0 ? n.ToString() : result;
    }

    public static string[] Sequence(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);

        // Размер известен заранее — сразу создаём массив нужной длины,
        // без промежуточного List и лишних копирований.
        var result = new string[count];
        for (var i = 0; i < count; i++)
        {
            result[i] = ForNumber(i + 1); // массив с 0, числа с 1 — классическое место для off-by-one
        }

        return result;
    }
}

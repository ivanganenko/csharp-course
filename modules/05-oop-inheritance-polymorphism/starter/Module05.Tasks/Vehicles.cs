namespace Module05.Tasks;

// Задача 05.2 — virtual / override / new.
// Сейчас методы в наследниках просто «повторяют» имена базовых — компилятор выдаёт warning CS0108
// («member hides inherited member»). Твоя задача — расставить модификаторы virtual / override / new
// и написать тела методов так, чтобы поведение совпало с описанием ниже (его проверяют тесты).
// Менять можно модификаторы и тела методов; имена, параметры и возвращаемые типы — нет.

/// <summary>Базовое транспортное средство.</summary>
public class Vehicle
{
    /// <summary>
    /// Вид транспорта. Должен определяться РЕАЛЬНЫМ типом объекта (runtime type):
    /// Vehicle v = new Car(); v.Kind() → "Car". Для самого Vehicle → "Vehicle".
    /// </summary>
    public string Kind() => throw new NotImplementedException();

    /// <summary>
    /// Количество колёс. Для Vehicle → "unknown".
    /// Должен определяться ТИПОМ ПЕРЕМЕННОЙ (compile-time type), а не объекта:
    /// Vehicle v = new Car(); v.Wheels() → "unknown", но Car c = new Car(); c.Wheels() → "4".
    /// </summary>
    public string Wheels() => throw new NotImplementedException();

    /// <summary>
    /// Описание — НЕ виртуальный метод, объявлен только здесь. Формат: "{Kind()} with {Wheels()} wheels".
    /// Подумай заранее: что вернёт new Car().Describe()? (Ответ проверяется тестом.)
    /// </summary>
    public string Describe() => throw new NotImplementedException();
}

/// <summary>Легковой автомобиль: Kind → "Car", Wheels (через переменную типа Car) → "4".</summary>
public class Car : Vehicle
{
    public string Kind() => throw new NotImplementedException();

    public string Wheels() => throw new NotImplementedException();
}

/// <summary>
/// Спорткар: Kind → "SportsCar" (в том числе через переменную типа Vehicle или Car).
/// Wheels не объявляет — наследует от Car.
/// </summary>
public sealed class SportsCar : Car
{
    public string Kind() => throw new NotImplementedException();
}

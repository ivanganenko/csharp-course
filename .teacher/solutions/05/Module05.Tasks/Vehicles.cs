namespace Module05.Tasks;

public class Vehicle
{
    // virtual: наследник может ПЕРЕОПРЕДЕЛИТЬ метод, и вызов через ссылку на базовый тип
    // пойдёт в реализацию реального типа объекта (через таблицу виртуальных методов, vtable).
    public virtual string Kind() => "Vehicle";

    // Не virtual: метод «привязывается» на этапе компиляции к типу переменной.
    public string Wheels() => "unknown";

    // Внутри Describe переменная this имеет тип Vehicle:
    // Kind() — виртуальный → версия реального типа; Wheels() — нет → версия Vehicle.
    public string Describe() => $"{Kind()} with {Wheels()} wheels";
}

public class Car : Vehicle
{
    // override: заменяет реализацию в vtable — работает и через ссылку Vehicle.
    public override string Kind() => "Car";

    // new: СКРЫВАЕТ метод базового класса (method hiding). Это отдельный метод с тем же именем;
    // через ссылку типа Vehicle вызовется Vehicle.Wheels(). Без new поведение было бы тем же,
    // но с warning CS0108 — new лишь говорит компилятору «я делаю это намеренно».
    public new string Wheels() => "4";
}

public sealed class SportsCar : Car
{
    // override можно делать от override: цепочка виртуальности продолжается, пока её не прервут new или sealed.
    public override string Kind() => "SportsCar";
}

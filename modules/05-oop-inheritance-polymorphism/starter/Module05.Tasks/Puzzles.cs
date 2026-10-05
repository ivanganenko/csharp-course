namespace Module05.Tasks;

// Задача 05.3 — головоломки «что выведет этот код?».
// Этот файл НЕ меняй. Прочитай каждую головоломку и запиши предсказание результата Run()
// в OutputPredictions.cs. Запускать и отлаживать код до того, как записал ответ, — нечестно :)

/// <summary>Порядок конструкторов, инициализаторов полей и виртуальный вызов из конструктора.</summary>
public static class ConstructorOrderPuzzle
{
    private static readonly List<string> Log = new();

    public static string Run()
    {
        Log.Clear();
        _ = new Derived();
        return string.Join(" | ", Log);
    }

    private class Base
    {
        public Base()
        {
            Log.Add("Base ctor");
            Init();
        }

        protected virtual void Init() => Log.Add("Base.Init");
    }

    private sealed class Derived : Base
    {
        private readonly string _a = Note("Derived field");
        private string? _b;

        public Derived()
        {
            _b = "set";
            Log.Add("Derived ctor");
        }

        protected override void Init() => Log.Add($"Derived.Init a={_a} b={_b ?? "null"}");

        private static string Note(string message)
        {
            Log.Add(message);
            return "set";
        }
    }
}

/// <summary>override vs new в трёхуровневой иерархии.</summary>
public static class NewVsOverridePuzzle
{
    public static string Run()
    {
        A x = new C();
        B y = new C();
        C z = new C();
        return $"{x.Who()} {y.Who()} {z.Who()}";
    }

    private class A
    {
        public virtual string Who() => "A";
    }

    private class B : A
    {
        public override string Who() => "B";
    }

    private sealed class C : B
    {
        public new string Who() => "C";
    }
}

/// <summary>Перегрузка (overloading) выбирается при компиляции, переопределение (overriding) — в runtime.</summary>
public static class OverloadVsOverridePuzzle
{
    public static string Run()
    {
        object boxed = "hello";
        Animal animal = new Cat();
        return $"{Describe(boxed)} {Describe("hello")} {animal.Sound()}";
    }

    private static string Describe(object value) => "object";

    private static string Describe(string value) => "string";

    private class Animal
    {
        public virtual string Sound() => "...";
    }

    private sealed class Cat : Animal
    {
        public override string Sound() => "Meow";
    }
}

/// <summary>Операторы is и as.</summary>
public static class IsAsPuzzle
{
    public static string Run()
    {
        Animal a = new Cat();
        Cat? asCat = a as Cat;
        Dog? asDog = a as Dog;
        return $"{a is Cat} {a is Animal} {asCat is null} {asDog is null}";
    }

    private class Animal
    {
    }

    private sealed class Cat : Animal
    {
    }

    private sealed class Dog : Animal
    {
    }
}

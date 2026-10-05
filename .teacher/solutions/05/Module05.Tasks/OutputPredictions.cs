namespace Module05.Tasks;

public static class OutputPredictions
{
    // 1) При new Derived() сначала выполняются инициализаторы полей Derived (Note → "Derived field"),
    // 2) затем вызывается конструктор Base ("Base ctor"),
    // 3) Base вызывает виртуальный Init() — а объект УЖЕ имеет тип Derived, поэтому вызывается Derived.Init.
    //    _a уже присвоено (инициализатор поля отработал до base-конструктора), а _b — ещё нет
    //    (тело конструктора Derived не начиналось) → "a=set b=null".
    // 4) Тело конструктора Derived ("Derived ctor").
    // Мораль: не вызывай виртуальные методы из конструктора — наследник увидит недоинициализированное состояние.
    public const string ConstructorOrder = "Derived field | Base ctor | Derived.Init a=set b=null | Derived ctor";

    // x (A) и y (B): виртуальный вызов находит последнее override в цепочке — это B.Who;
    // C.Who объявлен с new и в цепочку виртуальности не входит. z (C): статический тип C → C.Who.
    public const string NewVsOverride = "B B C";

    // Перегрузка выбирается КОМПИЛЯТОРОМ по статическому типу аргумента: boxed имеет тип object → Describe(object).
    // Переопределение выбирается в RUNTIME по реальному типу объекта: animal — это Cat → "Meow".
    public const string OverloadVsOverride = "object string Meow";

    // a is Cat → True; a is Animal → True; a as Cat → не null → False; a as Dog → null → True.
    public const string IsAndAs = "True True False True";
}

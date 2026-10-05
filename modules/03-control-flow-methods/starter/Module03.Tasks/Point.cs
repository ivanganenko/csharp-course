namespace Module03.Tasks;

/// <summary>
/// Точка на плоскости. Тип уже готов — менять его не нужно.
/// record автоматически умеет Deconstruct, поэтому с ним работает positional pattern: Point(0, _).
/// </summary>
public sealed record Point(int X, int Y);

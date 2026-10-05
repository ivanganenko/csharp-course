namespace Module15.Api.Configuration;

/// <summary>
/// Настройки трекера (Options pattern). Значения берутся из секции "TaskTracker" в appsettings.json
/// (а также из переменных окружения, аргументов командной строки и т.д.).
/// Значения по умолчанию здесь совпадают с appsettings.json — на случай, если секции нет.
/// </summary>
public sealed class TaskTrackerOptions
{
    public const string SectionName = "TaskTracker";

    /// <summary>Размер страницы, если клиент не передал pageSize.</summary>
    public int DefaultPageSize { get; set; } = 10;

    /// <summary>Максимально допустимый pageSize (защита от «дай мне миллион записей»).</summary>
    public int MaxPageSize { get; set; } = 50;
}

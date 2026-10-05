namespace Module15.Api.Validation;

public static class TaskValidator
{
    public const int MaxTitleLength = 100;
    public const int MaxDescriptionLength = 1000;
    public const int MinPriority = 1;
    public const int MaxPriority = 5;

    public static Dictionary<string, string[]> Validate(string? title, string? description, int priority)
    {
        // Собираем ВСЕ ошибки, а не выходим на первой: клиент (форма на фронтенде)
        // сможет подсветить все неверные поля за один запрос.
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(title))
        {
            errors["Title"] = ["Название обязательно."];
        }
        else if (title.Trim().Length > MaxTitleLength)
        {
            // Длину считаем после Trim — именно столько символов мы сохраним.
            errors["Title"] = [$"Название не длиннее {MaxTitleLength} символов."];
        }

        if (description is not null && description.Length > MaxDescriptionLength)
        {
            errors["Description"] = [$"Описание не длиннее {MaxDescriptionLength} символов."];
        }

        if (priority is < MinPriority or > MaxPriority)
        {
            // Relational + logical patterns (модуль 03) читаются как условие из ТЗ.
            errors["Priority"] = [$"Приоритет должен быть от {MinPriority} до {MaxPriority}."];
        }

        return errors;
    }
}

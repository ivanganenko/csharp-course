using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Module15.Api.Models;
using Module15.Api.Repositories;

namespace Module15.Tests;

/// <summary>
/// Общие помощники для тестов. Каждый тест-класс создаёт СВОЙ WebApplicationFactory на каждый тест
/// (xUnit создаёт новый экземпляр тест-класса на каждый тест) — так у тестов нет общего состояния
/// (in-memory репозиторий — Singleton внутри конкретного приложения).
/// </summary>
internal static class TestHelpers
{
    /// <summary>Кладёт задачу прямо в репозиторий приложения (в обход HTTP), чтобы подготовить данные.</summary>
    public static TaskItem Seed(
        WebApplicationFactory<Program> factory,
        string title,
        int priority = 3,
        bool isCompleted = false,
        string? description = null)
    {
        var repository = factory.Services.GetRequiredService<ITaskRepository>();
        return repository.Add(new TaskItem
        {
            Title = title,
            Description = description,
            Priority = priority,
            IsCompleted = isCompleted,
        });
    }

    public static ITaskRepository Repository(WebApplicationFactory<Program> factory) =>
        factory.Services.GetRequiredService<ITaskRepository>();

    /// <summary>Читает тело ответа как JSON-дерево (удобно для ProblemDetails).</summary>
    public static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    /// <summary>Есть ли в ProblemDetails ошибка валидации для поля key (имя поля сравниваем без учёта регистра).</summary>
    public static bool HasValidationError(JsonElement problem, string key)
    {
        if (!problem.TryGetProperty("errors", out var errors) || errors.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        foreach (var property in errors.EnumerateObject())
        {
            if (string.Equals(property.Name, key, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    public static string? GetString(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    public static int? GetInt(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Number
            ? value.GetInt32()
            : null;

    public static string? MediaType(HttpResponseMessage response) =>
        response.Content.Headers.ContentType?.MediaType;
}

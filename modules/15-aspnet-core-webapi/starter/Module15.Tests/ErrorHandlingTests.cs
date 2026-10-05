using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Module15.Api.ErrorHandling;

namespace Module15.Tests;

/// <summary>Задача 5. Глобальная обработка ошибок → ProblemDetails.</summary>
public sealed class ErrorHandlingTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory = new();
    private readonly HttpClient _client;

    public ErrorHandlingTests()
    {
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    // ---------- Unit-тесты CreateProblem ----------

    [Fact]
    public void CreateProblem_TaskNotFound_Returns404WithDetail()
    {
        var problem = GlobalExceptionHandler.CreateProblem(new TaskNotFoundException(42));

        Assert.Equal(404, problem.Status);
        Assert.Equal("Ресурс не найден", problem.Title);
        Assert.Contains("42", problem.Detail);
    }

    [Fact]
    public void CreateProblem_BusinessRule_Returns409WithMessageAsDetail()
    {
        var problem = GlobalExceptionHandler.CreateProblem(new BusinessRuleException("Нельзя так"));

        Assert.Equal(409, problem.Status);
        Assert.Equal("Нарушено бизнес-правило", problem.Title);
        Assert.Equal("Нельзя так", problem.Detail);
    }

    [Fact]
    public void CreateProblem_UnexpectedException_Returns500WithoutDetail()
    {
        var problem = GlobalExceptionHandler.CreateProblem(new InvalidOperationException("Password=secret"));

        Assert.Equal(500, problem.Status);
        Assert.Equal("Внутренняя ошибка сервера", problem.Title);
        Assert.Null(problem.Detail);
    }

    // ---------- Интеграционные тесты: обработчик подключён в pipeline ----------

    [Fact]
    public async Task Complete_MissingTask_Returns404ProblemDetails()
    {
        var response = await _client.PostAsync("/api/tasks/777/complete", content: null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", TestHelpers.MediaType(response));
        var problem = await TestHelpers.ReadJsonAsync(response);
        Assert.Equal(404, TestHelpers.GetInt(problem, "status"));
        Assert.Contains("777", TestHelpers.GetString(problem, "detail"));
    }

    [Fact]
    public async Task Complete_AlreadyCompletedTask_Returns409ProblemDetails()
    {
        var seeded = TestHelpers.Seed(_factory, "Уже сделано", isCompleted: true);

        var response = await _client.PostAsync($"/api/tasks/{seeded.Id}/complete", content: null);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("application/problem+json", TestHelpers.MediaType(response));
        var problem = await TestHelpers.ReadJsonAsync(response);
        Assert.Equal("Нарушено бизнес-правило", TestHelpers.GetString(problem, "title"));
    }

    [Fact]
    public async Task UnexpectedException_Returns500_WithoutLeakingSecrets()
    {
        var response = await _client.GetAsync("/api/diagnostics/crash");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("application/problem+json", TestHelpers.MediaType(response));
        Assert.DoesNotContain("SuperSecret123", body);
        var problem = await TestHelpers.ReadJsonAsync(response);
        Assert.Equal("Внутренняя ошибка сервера", TestHelpers.GetString(problem, "title"));
    }
}

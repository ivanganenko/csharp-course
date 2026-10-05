using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Module15.Api.Models;
using Module15.Api.Validation;

namespace Module15.Tests;

/// <summary>Задача 2. Валидация → 400 Bad Request + ValidationProblemDetails.</summary>
public sealed class TaskValidationTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory = new();
    private readonly HttpClient _client;

    public TaskValidationTests()
    {
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    // ---------- Unit-тесты TaskValidator (без HTTP) ----------

    [Fact]
    public void Validate_ValidInput_ReturnsNoErrors()
    {
        var errors = TaskValidator.Validate("Купить хлеб", null, 3);

        Assert.Empty(errors);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_MissingTitle_ReturnsTitleError(string? title)
    {
        var errors = TaskValidator.Validate(title, null, 3);

        Assert.True(errors.ContainsKey("Title"));
        Assert.NotEmpty(errors["Title"]);
    }

    [Fact]
    public void Validate_TitleExactlyMaxLength_IsValid_OneMoreIsError()
    {
        var maxTitle = new string('a', TaskValidator.MaxTitleLength);
        var tooLong = new string('a', TaskValidator.MaxTitleLength + 1);

        Assert.Empty(TaskValidator.Validate(maxTitle, null, 3));
        Assert.True(TaskValidator.Validate(tooLong, null, 3).ContainsKey("Title"));
    }

    [Fact]
    public void Validate_TitleWithSpacesAroundMaxLength_IsValid()
    {
        // Длина считается ПОСЛЕ Trim(): пробелы по краям — не часть названия.
        var title = "  " + new string('a', TaskValidator.MaxTitleLength) + "  ";

        Assert.Empty(TaskValidator.Validate(title, null, 3));
    }

    [Fact]
    public void Validate_TooLongDescription_ReturnsDescriptionError()
    {
        var description = new string('d', TaskValidator.MaxDescriptionLength + 1);

        var errors = TaskValidator.Validate("Задача", description, 3);

        Assert.True(errors.ContainsKey("Description"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public void Validate_PriorityOutOfRange_ReturnsPriorityError(int priority)
    {
        var errors = TaskValidator.Validate("Задача", null, priority);

        Assert.True(errors.ContainsKey("Priority"));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public void Validate_PriorityOnBoundary_IsValid(int priority)
    {
        Assert.Empty(TaskValidator.Validate("Задача", null, priority));
    }

    [Fact]
    public void Validate_SeveralProblems_ReturnsAllKeys()
    {
        var errors = TaskValidator.Validate("", null, 10);

        Assert.Equal(2, errors.Count);
        Assert.True(errors.ContainsKey("Title"));
        Assert.True(errors.ContainsKey("Priority"));
    }

    // ---------- Интеграционные тесты: эндпоинты используют валидатор ----------

    [Fact]
    public async Task Post_InvalidRequest_Returns400ProblemWithErrors_AndDoesNotCreateTask()
    {
        var request = new CreateTaskRequest { Title = "  ", Priority = 9 };

        var response = await _client.PostAsJsonAsync("/api/tasks", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", TestHelpers.MediaType(response));
        var problem = await TestHelpers.ReadJsonAsync(response);
        Assert.True(TestHelpers.HasValidationError(problem, "Title"));
        Assert.True(TestHelpers.HasValidationError(problem, "Priority"));
        Assert.Empty(TestHelpers.Repository(_factory).GetAll());
    }

    [Fact]
    public async Task Put_InvalidRequest_Returns400_AndKeepsTaskUnchanged()
    {
        var seeded = TestHelpers.Seed(_factory, "Исходное название", priority: 2);
        var request = new UpdateTaskRequest { Title = "", Priority = 2 };

        var response = await _client.PutAsJsonAsync($"/api/tasks/{seeded.Id}", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await TestHelpers.ReadJsonAsync(response);
        Assert.True(TestHelpers.HasValidationError(problem, "Title"));
        Assert.Equal("Исходное название", TestHelpers.Repository(_factory).GetById(seeded.Id)!.Title);
    }
}

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Module15.Api.Configuration;
using Module15.Api.Models;
using Module15.Api.Querying;

namespace Module15.Tests;

/// <summary>Задача 6 ⭐. Фильтрация и пагинация.</summary>
public sealed class TaskQueryTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory = new();
    private readonly HttpClient _client;

    public TaskQueryTests()
    {
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    // Набор данных для unit-тестов Apply.
    private static List<TaskItem> SampleTasks() =>
    [
        new() { Id = 1, Title = "Купить молоко", Priority = 2, IsCompleted = false },
        new() { Id = 2, Title = "Сдать отчёт", Description = "Квартальный отчёт для бухгалтерии", Priority = 5, IsCompleted = true },
        new() { Id = 3, Title = "Позвонить в банк", Priority = 5, IsCompleted = false },
        new() { Id = 4, Title = "Прочитать книгу", Description = "Про МОЛОКО и сыр", Priority = 1, IsCompleted = false },
        new() { Id = 5, Title = "Помыть машину", Priority = 3, IsCompleted = true },
    ];

    private static int[] Ids(PagedResponse<TaskResponse> page) => page.Items.Select(t => t.Id).ToArray();

    // ---------- Validate ----------

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("all")]
    [InlineData("active")]
    [InlineData("COMPLETED")]
    public void Validate_KnownStatus_NoErrors(string? status)
    {
        var errors = TaskQuery.Validate(new TaskQueryParameters(status, null, 1, 10), maxPageSize: 50);

        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_UnknownStatus_ReturnsStatusError()
    {
        var errors = TaskQuery.Validate(new TaskQueryParameters("done-ish", null, 1, 10), maxPageSize: 50);

        Assert.True(errors.ContainsKey("Status"));
    }

    [Theory]
    [InlineData(0, 10, "Page")]
    [InlineData(-3, 10, "Page")]
    [InlineData(1, 0, "PageSize")]
    [InlineData(1, 51, "PageSize")]
    public void Validate_BadPaging_ReturnsError(int page, int pageSize, string expectedKey)
    {
        var errors = TaskQuery.Validate(new TaskQueryParameters(null, null, page, pageSize), maxPageSize: 50);

        Assert.True(errors.ContainsKey(expectedKey));
    }

    [Fact]
    public void Validate_PageSizeEqualToMax_IsValid()
    {
        Assert.Empty(TaskQuery.Validate(new TaskQueryParameters(null, null, 1, 50), maxPageSize: 50));
    }

    // ---------- Apply ----------

    [Fact]
    public void Apply_NoFilters_SortsByPriorityDescThenId_AndCountsAll()
    {
        var result = TaskQuery.Apply(SampleTasks(), new TaskQueryParameters(null, null, 1, 10));

        Assert.Equal(new[] { 2, 3, 5, 1, 4 }, Ids(result));
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
    }

    [Theory]
    [InlineData("active", new[] { 3, 1, 4 })]
    [InlineData("Completed", new[] { 2, 5 })]
    [InlineData("all", new[] { 2, 3, 5, 1, 4 })]
    public void Apply_StatusFilter(string status, int[] expectedIds)
    {
        var result = TaskQuery.Apply(SampleTasks(), new TaskQueryParameters(status, null, 1, 10));

        Assert.Equal(expectedIds, Ids(result));
        Assert.Equal(expectedIds.Length, result.TotalCount);
    }

    [Fact]
    public void Apply_Search_IgnoresCase_LooksInTitleAndDescription()
    {
        var result = TaskQuery.Apply(SampleTasks(), new TaskQueryParameters(null, "  молоко ", 1, 10));

        // "Купить молоко" (в названии) и "Про МОЛОКО и сыр" (в описании, другой регистр).
        Assert.Equal(new[] { 1, 4 }, Ids(result));
    }

    [Fact]
    public void Apply_StatusAndSearchTogether()
    {
        var result = TaskQuery.Apply(SampleTasks(), new TaskQueryParameters("completed", "отчёт", 1, 10));

        Assert.Equal(new[] { 2 }, Ids(result));
    }

    [Fact]
    public void Apply_SecondPage_ReturnsNextItems_TotalCountIsBeforePaging()
    {
        var result = TaskQuery.Apply(SampleTasks(), new TaskQueryParameters(null, null, 2, 2));

        Assert.Equal(new[] { 5, 1 }, Ids(result));
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
    }

    [Fact]
    public void Apply_PageBeyondEnd_ReturnsEmptyItems()
    {
        var result = TaskQuery.Apply(SampleTasks(), new TaskQueryParameters(null, null, 10, 2));

        Assert.Empty(result.Items);
        Assert.Equal(5, result.TotalCount);
    }

    [Fact]
    public void Apply_InvalidPage_Throws()
    {
        Assert.ThrowsAny<ArgumentOutOfRangeException>(
            () => TaskQuery.Apply(SampleTasks(), new TaskQueryParameters(null, null, 0, 10)));
    }

    // ---------- Интеграция: эндпоинт GET /api/tasks ----------

    [Fact]
    public async Task GetTasks_WithFilterAndPaging_Returns200Page()
    {
        TestHelpers.Seed(_factory, "Задача A", priority: 1);
        TestHelpers.Seed(_factory, "Задача B", priority: 4);
        TestHelpers.Seed(_factory, "Задача C", priority: 2, isCompleted: true);
        TestHelpers.Seed(_factory, "Задача D", priority: 4);

        var page = await _client.GetFromJsonAsync<PagedResponse<TaskResponse>>("/api/tasks?status=active&page=1&pageSize=2");

        Assert.NotNull(page);
        Assert.Equal(new[] { "Задача B", "Задача D" }, page.Items.Select(t => t.Title).ToArray());
        Assert.Equal(3, page.TotalCount);
        Assert.Equal(2, page.PageSize);
    }

    [Fact]
    public async Task GetTasks_NoParameters_UsesDefaults()
    {
        for (var i = 1; i <= 12; i++)
        {
            TestHelpers.Seed(_factory, $"Задача {i}");
        }

        var page = await _client.GetFromJsonAsync<PagedResponse<TaskResponse>>("/api/tasks");

        Assert.NotNull(page);
        Assert.Equal(1, page.Page);
        Assert.Equal(10, page.PageSize);
        Assert.Equal(10, page.Items.Count);
        Assert.Equal(12, page.TotalCount);
    }

    [Fact]
    public async Task GetTasks_InvalidStatus_Returns400()
    {
        var response = await _client.GetAsync("/api/tasks?status=whatever");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await TestHelpers.ReadJsonAsync(response);
        Assert.True(TestHelpers.HasValidationError(problem, "Status"));
    }

    [Fact]
    public async Task GetTasks_PageSizeAboveConfiguredMax_Returns400()
    {
        // Подменяем настройку только для этого теста: MaxPageSize = 3.
        // PostConfigure выполняется ПОСЛЕ всех Configure — поэтому перекрывает appsettings.json.
        // Производную фабрику не диспозим вручную: её освободит родительская _factory в Dispose().
        var factory = _factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
                services.PostConfigure<TaskTrackerOptions>(options => options.MaxPageSize = 3)));
        using var client = factory.CreateClient();

        var ok = await client.GetAsync("/api/tasks?pageSize=3");
        var tooBig = await client.GetAsync("/api/tasks?pageSize=4");

        Assert.Equal(HttpStatusCode.OK, ok.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, tooBig.StatusCode);
    }
}

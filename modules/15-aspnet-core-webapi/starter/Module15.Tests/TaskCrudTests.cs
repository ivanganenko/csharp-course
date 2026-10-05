using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Module15.Api.Models;

namespace Module15.Tests;

/// <summary>Задача 1. CRUD-эндпоинты: POST / PUT / DELETE.</summary>
public sealed class TaskCrudTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory = new();
    private readonly HttpClient _client;

    public TaskCrudTests()
    {
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task Create_ValidRequest_Returns201WithLocationAndBody()
    {
        var request = new CreateTaskRequest { Title = "Купить молоко", Description = "2 литра", Priority = 2 };

        var response = await _client.PostAsJsonAsync("/api/tasks", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<TaskResponse>();
        Assert.NotNull(created);
        Assert.True(created.Id > 0);
        Assert.Equal("Купить молоко", created.Title);
        Assert.Equal("2 литра", created.Description);
        Assert.Equal(2, created.Priority);
        Assert.False(created.IsCompleted);
        Assert.NotNull(response.Headers.Location);
        Assert.EndsWith($"/api/tasks/{created.Id}", response.Headers.Location.ToString());
    }

    [Fact]
    public async Task Create_TitleAndDescriptionWithSpaces_AreTrimmed_EmptyDescriptionBecomesNull()
    {
        var request = new CreateTaskRequest { Title = "  Позвонить маме  ", Description = "   ", Priority = 3 };

        var response = await _client.PostAsJsonAsync("/api/tasks", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<TaskResponse>();
        Assert.NotNull(created);
        Assert.Equal("Позвонить маме", created.Title);
        Assert.Null(created.Description);
    }

    [Fact]
    public async Task Create_ThenGetById_ReturnsSameTask()
    {
        var response = await _client.PostAsJsonAsync("/api/tasks", new CreateTaskRequest { Title = "Выучить DI", Priority = 5 });
        var created = await response.Content.ReadFromJsonAsync<TaskResponse>();
        Assert.NotNull(created);

        var loaded = await _client.GetFromJsonAsync<TaskResponse>($"/api/tasks/{created.Id}");

        Assert.Equal(created, loaded);
    }

    [Fact]
    public async Task Create_TwoTasks_GetDifferentIds()
    {
        var first = await (await _client.PostAsJsonAsync("/api/tasks", new CreateTaskRequest { Title = "Первая" }))
            .Content.ReadFromJsonAsync<TaskResponse>();
        var second = await (await _client.PostAsJsonAsync("/api/tasks", new CreateTaskRequest { Title = "Вторая" }))
            .Content.ReadFromJsonAsync<TaskResponse>();

        Assert.NotNull(first);
        Assert.NotNull(second);
        Assert.NotEqual(first.Id, second.Id);
        Assert.Equal(2, TestHelpers.Repository(_factory).GetAll().Count);
    }

    [Fact]
    public async Task Update_ExistingTask_Returns204AndChangesTask()
    {
        var seeded = TestHelpers.Seed(_factory, "Старое название", priority: 1);
        var request = new UpdateTaskRequest { Title = " Новое название ", Description = "Описание", Priority = 4, IsCompleted = true };

        var response = await _client.PutAsJsonAsync($"/api/tasks/{seeded.Id}", request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var stored = TestHelpers.Repository(_factory).GetById(seeded.Id);
        Assert.NotNull(stored);
        Assert.Equal("Новое название", stored.Title);
        Assert.Equal("Описание", stored.Description);
        Assert.Equal(4, stored.Priority);
        Assert.True(stored.IsCompleted);
    }

    [Fact]
    public async Task Update_MissingTask_Returns404()
    {
        var request = new UpdateTaskRequest { Title = "Что-то", Priority = 3 };

        var response = await _client.PutAsJsonAsync("/api/tasks/999", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ExistingTask_Returns204AndTaskIsGone()
    {
        var seeded = TestHelpers.Seed(_factory, "Удали меня");

        var response = await _client.DeleteAsync($"/api/tasks/{seeded.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Null(TestHelpers.Repository(_factory).GetById(seeded.Id));
        var getResponse = await _client.GetAsync($"/api/tasks/{seeded.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_MissingTask_Returns404()
    {
        var response = await _client.DeleteAsync("/api/tasks/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}

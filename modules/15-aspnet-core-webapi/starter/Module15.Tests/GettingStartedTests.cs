using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Module15.Api.Models;

namespace Module15.Tests;

/// <summary>
/// Образец интеграционных тестов. Эти тесты ЗЕЛЁНЫЕ с самого начала — они проверяют готовые эндпоинты
/// и показывают, как устроен WebApplicationFactory: приложение поднимается в памяти,
/// HttpClient ходит в него без сети и портов.
/// </summary>
public sealed class GettingStartedTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory = new();
    private readonly HttpClient _client;

    public GettingStartedTests()
    {
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task Ping_ReturnsPong()
    {
        var text = await _client.GetStringAsync("/api/ping");

        Assert.Equal("pong", text);
    }

    [Fact]
    public async Task GetById_ExistingTask_Returns200WithTask()
    {
        var seeded = TestHelpers.Seed(_factory, "Написать README", priority: 4);

        var task = await _client.GetFromJsonAsync<TaskResponse>($"/api/tasks/{seeded.Id}");

        Assert.NotNull(task);
        Assert.Equal(seeded.Id, task.Id);
        Assert.Equal("Написать README", task.Title);
        Assert.Equal(4, task.Priority);
        Assert.False(task.IsCompleted);
    }

    [Fact]
    public async Task GetById_MissingTask_Returns404()
    {
        var response = await _client.GetAsync("/api/tasks/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Complete_ExistingTask_Returns200AndMarksCompleted()
    {
        var seeded = TestHelpers.Seed(_factory, "Полить цветы");

        var response = await _client.PostAsync($"/api/tasks/{seeded.Id}/complete", content: null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(TestHelpers.Repository(_factory).GetById(seeded.Id)!.IsCompleted);
    }
}

using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Module15.Api.Middleware;

namespace Module15.Tests;

/// <summary>Задача 3. Свой middleware: заголовок X-Request-Id у каждого ответа.</summary>
public sealed class RequestIdMiddlewareTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory = new();
    private readonly HttpClient _client;

    public RequestIdMiddlewareTests()
    {
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Theory]
    [InlineData("abc-123", true)]
    [InlineData("Order_42", true)]
    [InlineData("0f8fad5bd9cb469fa16570867728950e", true)]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("has space", false)]
    [InlineData("<script>", false)]
    [InlineData("идентификатор", false)]
    public void IsValidIncomingId_ReturnsExpected(string? value, bool expected)
    {
        Assert.Equal(expected, RequestIdMiddleware.IsValidIncomingId(value));
    }

    [Fact]
    public void IsValidIncomingId_LengthBoundary()
    {
        Assert.True(RequestIdMiddleware.IsValidIncomingId(new string('a', RequestIdMiddleware.MaxIncomingIdLength)));
        Assert.False(RequestIdMiddleware.IsValidIncomingId(new string('a', RequestIdMiddleware.MaxIncomingIdLength + 1)));
    }

    [Fact]
    public async Task Request_WithoutHeader_ResponseHasGeneratedGuid()
    {
        var response = await _client.GetAsync("/api/ping");

        var id = GetRequestId(response);
        Assert.True(Guid.TryParseExact(id, "N", out _), $"Ожидался Guid в формате N, получено: '{id}'");
    }

    [Fact]
    public async Task Request_WithValidHeader_EchoesIt()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/ping");
        request.Headers.TryAddWithoutValidation(RequestIdMiddleware.HeaderName, "client-req-42");

        var response = await _client.SendAsync(request);

        Assert.Equal("client-req-42", GetRequestId(response));
    }

    [Fact]
    public async Task Request_WithInvalidHeader_GeneratesNewId()
    {
        var tooLong = new string('x', RequestIdMiddleware.MaxIncomingIdLength + 10);
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/ping");
        request.Headers.TryAddWithoutValidation(RequestIdMiddleware.HeaderName, tooLong);

        var response = await _client.SendAsync(request);

        var id = GetRequestId(response);
        Assert.NotEqual(tooLong, id);
        Assert.True(Guid.TryParseExact(id, "N", out _));
    }

    [Fact]
    public async Task TwoRequests_GetDifferentIds()
    {
        var first = GetRequestId(await _client.GetAsync("/api/ping"));
        var second = GetRequestId(await _client.GetAsync("/api/ping"));

        Assert.NotEqual(first, second);
    }

    [Fact]
    public async Task UnknownRoute_404_StillHasHeader()
    {
        var response = await _client.GetAsync("/api/no-such-route");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.False(string.IsNullOrEmpty(GetRequestId(response)));
    }

    private static string GetRequestId(HttpResponseMessage response)
    {
        Assert.True(
            response.Headers.TryGetValues(RequestIdMiddleware.HeaderName, out var values),
            $"В ответе нет заголовка {RequestIdMiddleware.HeaderName}");
        return Assert.Single(values);
    }
}

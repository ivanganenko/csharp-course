using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Module15.Api.Diagnostics;
using Module15.Api.Models;

namespace Module15.Tests;

/// <summary>Задача 4. DI lifetimes: Singleton vs Scoped vs Transient.</summary>
public sealed class LifetimeTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory = new();
    private readonly HttpClient _client;

    public LifetimeTests()
    {
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Theory]
    [InlineData(typeof(SingletonCounter), ServiceLifetime.Singleton)]
    [InlineData(typeof(ScopedCounter), ServiceLifetime.Scoped)]
    [InlineData(typeof(TransientCounter), ServiceLifetime.Transient)]
    public void AddLifetimeDemo_RegistersServiceWithExpectedLifetime(Type serviceType, ServiceLifetime expected)
    {
        var services = new ServiceCollection();

        services.AddLifetimeDemo();

        var descriptor = Assert.Single(services, d => d.ServiceType == serviceType);
        Assert.Equal(expected, descriptor.Lifetime);
    }

    [Fact]
    public void AddLifetimeDemo_ReturnsSameCollection_ForChaining()
    {
        var services = new ServiceCollection();

        var returned = services.AddLifetimeDemo();

        Assert.Same(services, returned);
    }

    [Fact]
    public void Scoped_SameInstanceWithinScope_DifferentAcrossScopes()
    {
        using var provider = new ServiceCollection().AddLifetimeDemo().BuildServiceProvider(validateScopes: true);
        using var scope1 = provider.CreateScope();
        using var scope2 = provider.CreateScope();

        var a = scope1.ServiceProvider.GetRequiredService<ScopedCounter>();
        var b = scope1.ServiceProvider.GetRequiredService<ScopedCounter>();
        var c = scope2.ServiceProvider.GetRequiredService<ScopedCounter>();

        Assert.Same(a, b);
        Assert.NotSame(a, c);
    }

    [Fact]
    public void Singleton_SameInstanceAcrossScopes_TransientAlwaysNew()
    {
        using var provider = new ServiceCollection().AddLifetimeDemo().BuildServiceProvider(validateScopes: true);
        using var scope1 = provider.CreateScope();
        using var scope2 = provider.CreateScope();

        Assert.Same(
            scope1.ServiceProvider.GetRequiredService<SingletonCounter>(),
            scope2.ServiceProvider.GetRequiredService<SingletonCounter>());
        Assert.NotSame(
            scope1.ServiceProvider.GetRequiredService<TransientCounter>(),
            scope1.ServiceProvider.GetRequiredService<TransientCounter>());
    }

    [Fact]
    public async Task CountersEndpoint_FirstRequest_ReturnsTwoTwoOne()
    {
        var counters = await _client.GetFromJsonAsync<CountersResponse>("/api/diagnostics/counters");

        // Singleton: один объект увеличили дважды → 2.
        // Scoped: в рамках запроса тот же объект → 2.
        // Transient: второй экземпляр — новый объект, его увеличили один раз → 1.
        Assert.Equal(new CountersResponse(Singleton: 2, Scoped: 2, Transient: 1), counters);
    }

    [Fact]
    public async Task CountersEndpoint_SecondRequest_SingletonKeepsGrowing_ScopedStartsOver()
    {
        await _client.GetFromJsonAsync<CountersResponse>("/api/diagnostics/counters");

        var second = await _client.GetFromJsonAsync<CountersResponse>("/api/diagnostics/counters");

        Assert.Equal(new CountersResponse(Singleton: 4, Scoped: 2, Transient: 1), second);
    }
}

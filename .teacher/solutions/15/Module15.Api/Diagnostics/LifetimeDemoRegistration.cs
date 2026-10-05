namespace Module15.Api.Diagnostics;

public static class LifetimeDemoRegistration
{
    public static IServiceCollection AddLifetimeDemo(this IServiceCollection services)
    {
        // Singleton — один объект на всё время жизни приложения (кэш, конфигурация, счётчики).
        services.AddSingleton<SingletonCounter>();

        // Scoped — один объект на scope; в ASP.NET Core scope = HTTP-запрос
        // (типичный пример — DbContext: единица работы в рамках запроса).
        services.AddScoped<ScopedCounter>();

        // Transient — новый объект при каждом resolve (лёгкие stateless-сервисы).
        services.AddTransient<TransientCounter>();

        // Возвращаем ту же коллекцию — fluent-стиль, как у всех AddXxx в ASP.NET Core.
        return services;
    }
}

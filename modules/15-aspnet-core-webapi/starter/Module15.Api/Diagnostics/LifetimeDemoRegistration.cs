namespace Module15.Api.Diagnostics;

/// <summary>
/// Задача 4. Регистрация сервисов с разными lifetimes.
/// Метод уже вызывается в Program.cs: builder.Services.AddLifetimeDemo().
/// </summary>
public static class LifetimeDemoRegistration
{
    /// <summary>
    /// Зарегистрируй в контейнере:
    /// <list type="bullet">
    /// <item><see cref="SingletonCounter"/> — Singleton (один экземпляр на всё приложение);</item>
    /// <item><see cref="ScopedCounter"/> — Scoped (один экземпляр на HTTP-запрос);</item>
    /// <item><see cref="TransientCounter"/> — Transient (новый экземпляр при каждом запросе к контейнеру).</item>
    /// </list>
    /// Регистрируй сам класс как сервис (без интерфейса). Верни <paramref name="services"/>,
    /// чтобы вызовы можно было «цеплять» (fluent API) — так устроены все AddXxx в ASP.NET Core.
    /// </summary>
    public static IServiceCollection AddLifetimeDemo(this IServiceCollection services)
    {
        // TODO (задача 4).
        // Почему здесь не NotImplementedException: метод вызывается при старте приложения —
        // исключение не дало бы запуститься API, и упали бы тесты ВСЕХ задач модуля.
        return services;
    }
}

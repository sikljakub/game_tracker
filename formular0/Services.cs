using System;
using Microsoft.Extensions.DependencyInjection;
using formular0.Repositories;
using formular0.ViewModels;

namespace formular0;

public static class Services
{
    private static ServiceProvider? _provider;

    public static void Initialize()
    {
        var services = new ServiceCollection();
        var connectionString = BuildConnectionString();

        services.AddSingleton<IPlatformRepository>(_ => new PlatformRepository(connectionString));
        services.AddSingleton<IGameRepository>(_ => new GameRepository(connectionString));
        services.AddSingleton<IGameSessionRepository>(_ => new GameSessionRepository(connectionString));

        services.AddTransient<MainWindowViewModel>();

        _provider = services.BuildServiceProvider();
    }

    public static T Get<T>() where T : notnull
    {
        if (_provider is null)
            throw new InvalidOperationException(
                "Services nejsou inicializovány. Zavolej Initialize() v Program.cs.");
        return _provider.GetRequiredService<T>();
    }

    public static string GetConnectionString() => BuildConnectionString();

    private static string BuildConnectionString()
    {
        var host     = Env("HOST",        "localhost");
        var port     = Env("PORT",        "5432");
        var database = Env("DATABASE",    null);
        var username = Env("DB_USER",     null);
        var password = Env("DB_PASSWORD", null);

        return $"Host={host};Port={port};Database={database};Username={username};Password={password};Ssl Mode=Disable;Timeout=15;";
    }

    private static string Env(string key, string? fallback)
    {
        var value = Environment.GetEnvironmentVariable(key);
        if (value is not null) return value;
        if (fallback is not null) return fallback;
        throw new InvalidOperationException(
            $"Chybí povinná env proměnná '{key}'. Zkontroluj .env soubor.");
    }
}

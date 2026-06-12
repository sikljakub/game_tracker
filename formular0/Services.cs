using System;
using Microsoft.Extensions.DependencyInjection;
using formular0.Repositories;
using formular0.ViewModels;

namespace formular0;

// Statická třída pro Dependency Injection (DI)
// DI = způsob jak aplikace automaticky "dostane" potřebné závislosti
// místo ručního vytváření objektů (new GameRepository(...)) to udělá DI za nás
public static class Services
{
    // kontejner který drží všechny zaregistrované služby
    private static ServiceProvider? _provider;

    // Zaregistruje všechny služby a sestaví DI kontejner
    public static void Initialize()
    {
        var services = new ServiceCollection();
        var connectionString = BuildConnectionString();

        // AddSingleton = vytvoří se JEDNA instance pro celý běh aplikace
        // pokaždé kdy někdo požádá o IPlatformRepository, dostane stejnou instanci
        services.AddSingleton<IPlatformRepository>(_ => new PlatformRepository(connectionString));
        services.AddSingleton<IGameRepository>(_ => new GameRepository(connectionString));
        services.AddSingleton<IGameSessionRepository>(_ => new GameSessionRepository(connectionString));

        // AddTransient = nová instance pokaždé když se o ni požádá
        services.AddTransient<MainWindowViewModel>();

        // sestaví kontejner — od teď lze volat Get<T>()
        _provider = services.BuildServiceProvider();
    }

    // Vrátí instanci požadované služby z DI kontejneru
    // Použití: Services.Get<MainWindowViewModel>()
    public static T Get<T>() where T : notnull
    {
        if (_provider is null)
            throw new InvalidOperationException(
                "Services nejsou inicializovány. Zavolej Initialize() v Program.cs.");
        return _provider.GetRequiredService<T>();
    }

    // Vrátí connection string pro DbUp (používá se v Program.cs)
    public static string GetConnectionString() => BuildConnectionString();

    // Sestaví connection string pro PostgreSQL z env proměnných
    private static string BuildConnectionString()
    {
        var host     = Env("HOST",        "localhost"); // výchozí hodnota localhost
        var port     = Env("PORT",        "5432");      // výchozí port PostgreSQL
        var database = Env("DATABASE",    null);        // null = povinné, bez výchozí hodnoty
        var username = Env("DB_USER",     null);
        var password = Env("DB_PASSWORD", null);

        return $"Host={host};Port={port};Database={database};Username={username};Password={password};Ssl Mode=Disable;Timeout=15;";
    }

    // Přečte env proměnnou, nebo vrátí výchozí hodnotu, nebo vyhodí výjimku
    private static string Env(string key, string? fallback)
    {
        var value = Environment.GetEnvironmentVariable(key);
        if (value is not null) return value;       // proměnná existuje — vrátí ji
        if (fallback is not null) return fallback; // má výchozí hodnotu — vrátí ji
        throw new InvalidOperationException(       // povinná proměnná chybí — chyba
            $"Chybí povinná env proměnná '{key}'. Zkontroluj .env soubor.");
    }
}

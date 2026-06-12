using System;
using System.IO;
using System.Reflection;
using Avalonia;
using Dapper;
using DbUp;

namespace formular0;

class Program
{
    public static void Main(string[] args)
    {
        try
        {
            // Dapper automaticky převádí snake_case sloupce (platform_name) na PascalCase vlastnosti (PlatformName)
            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

            LoadEnvFile();          // 1. načte přihlašovací údaje z .env souboru
            Services.Initialize();  // 2. sestaví DI kontejner (zaregistruje repozitáře)
            RunMigrations();        // 3. spustí SQL skripty a vytvoří tabulky v databázi

            // 4. spustí Avalonia GUI aplikaci
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            // pokud cokoliv selže (chybí DB, špatné heslo...), vypíše chybu
            Console.WriteLine(ex.ToString());
            Console.ReadLine();
        }
    }

    // Konfigurace Avalonia frameworku
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()   // automaticky detekuje Windows/Linux/Mac
            .LogToTrace();

    // Spustí DbUp migrace — SQL skripty ze složky Scripts/
    private static void RunMigrations()
    {
        var connectionString = Services.GetConnectionString();

        var upgrader = DeployChanges.To
            .PostgresqlDatabase(connectionString)
            // vezme SQL soubory zabudované v .exe (embedded resources)
            .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
            // každý skript se spustí v samostatné transakci
            .WithTransactionPerScript()
            .LogToConsole()
            .Build();

        var result = upgrader.PerformUpgrade();

        if (!result.Successful)
            throw new Exception($"Migrace selhala: {result.Error.Message}");
    }

    // Načte .env soubor a nastaví proměnné prostředí
    // Hledá .env v aktuální složce a až 5 úrovní výše
    private static void LoadEnvFile()
    {
        var dir = Directory.GetCurrentDirectory();

        for (int i = 0; i < 5; i++)
        {
            var path = Path.Combine(dir, ".env");
            if (File.Exists(path))
            {
                foreach (var line in File.ReadAllLines(path))
                {
                    // přeskočí prázdné řádky a komentáře (#)
                    if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith('#'))
                        continue;

                    // rozdělí řádek na klíč a hodnotu podle prvního '='
                    var sep = line.IndexOf('=');
                    if (sep < 0) continue;

                    var key   = line[..sep].Trim();
                    var value = line[(sep + 1)..].Trim();

                    // nenastaví pokud proměnná už existuje (systémové env mají přednost)
                    if (Environment.GetEnvironmentVariable(key) is null)
                        Environment.SetEnvironmentVariable(key, value);
                }
                return;
            }
            // jdi o složku výš
            dir = Directory.GetParent(dir)?.FullName ?? dir;
        }
    }
}

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
            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
            LoadEnvFile();          // 1. načti .env
            Services.Initialize();  // 2. sestav DI kontejner
            RunMigrations();        // 3. spusť DB migrace (DbUp)

            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            Console.ReadLine();
        }
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();

    private static void RunMigrations()
    {
        var connectionString = Services.GetConnectionString();

        var upgrader = DeployChanges.To
            .PostgresqlDatabase(connectionString)
            .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
            .WithTransactionPerScript()
            .LogToConsole()
            .Build();

        var result = upgrader.PerformUpgrade();

        if (!result.Successful)
            throw new Exception($"Migrace selhala: {result.Error.Message}");
    }

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
                    if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith('#'))
                        continue;

                    var sep = line.IndexOf('=');
                    if (sep < 0) continue;

                    var key   = line[..sep].Trim();
                    var value = line[(sep + 1)..].Trim();

                    if (Environment.GetEnvironmentVariable(key) is null)
                        Environment.SetEnvironmentVariable(key, value);
                }
                return;
            }
            dir = Directory.GetParent(dir)?.FullName ?? dir;
        }
    }
}

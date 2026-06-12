using System.Collections.Generic;
using Dapper;
using Npgsql;
using formular0.Models;

namespace formular0.Repositories;

// Implementace IGameRepository — přistupuje k tabulce games v PostgreSQL přes Dapper
public class GameRepository : IGameRepository
{
    private readonly string _cs; // connection string uložený při vytvoření

    public GameRepository(string connectionString) => _cs = connectionString;

    // vytvoří nové připojení k databázi
    private NpgsqlConnection Connect() => new(_cs);

    // Vrátí všechny hry — JOIN s platforms aby se získal název platformy
    public List<Game> GetAll()
    {
        using var conn = Connect(); // using = automaticky zavře připojení po dokončení
        return conn.Query<Game>("""
            SELECT g.id, g.title, g.platform_id, p.name AS platform_name,
                   g.release_year, g.note
            FROM games g
            JOIN platforms p ON p.id = g.platform_id
            ORDER BY g.title
            """).AsList();
        // Dapper automaticky namapuje sloupce SQL na vlastnosti třídy Game
        // platform_name → PlatformName (díky MatchNamesWithUnderscores)
    }

    // Přidá novou hru do databáze
    public void Add(Game game)
    {
        using var conn = Connect();
        conn.Execute("""
            INSERT INTO games (title, platform_id, release_year, note)
            VALUES (@Title, @PlatformId, @ReleaseYear, @Note)
            """, game); // Dapper vezme hodnoty z objektu game (@Title = game.Title atd.)
    }

    // Aktualizuje existující hru
    public void Update(Game game)
    {
        using var conn = Connect();
        conn.Execute("""
            UPDATE games
            SET title = @Title, platform_id = @PlatformId,
                release_year = @ReleaseYear, note = @Note
            WHERE id = @Id
            """, game);
    }

    // Smaže hru podle ID (game_sessions se smažou automaticky — ON DELETE CASCADE)
    public void Delete(int id)
    {
        using var conn = Connect();
        conn.Execute("DELETE FROM games WHERE id = @id", new { id });
    }

    // Vrátí celkový počet odehraných hodin přes všechny hry
    // COALESCE = pokud není žádná relace, vrátí 0 místo NULL
    public decimal GetTotalHours()
    {
        using var conn = Connect();
        return conn.QuerySingle<decimal>(
            "SELECT COALESCE(SUM(hours_played), 0) FROM game_sessions");
    }

    // Vrátí název nejhranější hry (podle součtu hodin)
    // Vrátí null pokud nejsou žádné herní relace
    public string? GetMostPlayedGame()
    {
        using var conn = Connect();
        return conn.QueryFirstOrDefault<string>("""
            SELECT g.title
            FROM games g
            JOIN game_sessions s ON s.game_id = g.id
            GROUP BY g.id, g.title
            ORDER BY SUM(s.hours_played) DESC
            LIMIT 1
            """);
    }
}

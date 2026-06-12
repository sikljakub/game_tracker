using System.Collections.Generic;
using Dapper;
using Npgsql;
using formular0.Models;

namespace formular0.Repositories;

public class GameRepository : IGameRepository
{
    private readonly string _cs;

    public GameRepository(string connectionString) => _cs = connectionString;

    private NpgsqlConnection Connect() => new(_cs);

    public List<Game> GetAll()
    {
        using var conn = Connect();
        return conn.Query<Game>("""
            SELECT g.id, g.title, g.platform_id, p.name AS platform_name,
                   g.release_year, g.note
            FROM games g
            JOIN platforms p ON p.id = g.platform_id
            ORDER BY g.title
            """).AsList();
    }

    public void Add(Game game)
    {
        using var conn = Connect();
        conn.Execute("""
            INSERT INTO games (title, platform_id, release_year, note)
            VALUES (@Title, @PlatformId, @ReleaseYear, @Note)
            """, game);
    }

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

    public void Delete(int id)
    {
        using var conn = Connect();
        conn.Execute("DELETE FROM games WHERE id = @id", new { id });
    }

    public decimal GetTotalHours()
    {
        using var conn = Connect();
        return conn.QuerySingle<decimal>(
            "SELECT COALESCE(SUM(hours_played), 0) FROM game_sessions");
    }

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

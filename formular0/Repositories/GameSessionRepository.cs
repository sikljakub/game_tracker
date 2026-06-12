using System.Collections.Generic;
using Dapper;
using Npgsql;
using formular0.Models;

namespace formular0.Repositories;

public class GameSessionRepository : IGameSessionRepository
{
    private readonly string _cs;

    public GameSessionRepository(string connectionString) => _cs = connectionString;

    private NpgsqlConnection Connect() => new(_cs);

    public List<GameSession> GetByGame(int gameId)
    {
        using var conn = Connect();
        return conn.Query<GameSession>("""
            SELECT id, game_id, played_on, hours_played, note
            FROM game_sessions
            WHERE game_id = @gameId
            ORDER BY played_on DESC
            """, new { gameId }).AsList();
    }

    public void Add(GameSession session)
    {
        using var conn = Connect();
        conn.Execute("""
            INSERT INTO game_sessions (game_id, played_on, hours_played, note)
            VALUES (@GameId, @PlayedOn, @HoursPlayed, @Note)
            """, session);
    }

    public void Update(GameSession session)
    {
        using var conn = Connect();
        conn.Execute("""
            UPDATE game_sessions
            SET played_on = @PlayedOn, hours_played = @HoursPlayed, note = @Note
            WHERE id = @Id
            """, session);
    }

    public void Delete(int id)
    {
        using var conn = Connect();
        conn.Execute("DELETE FROM game_sessions WHERE id = @id", new { id });
    }
}

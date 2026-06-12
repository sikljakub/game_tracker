using System.Collections.Generic;
using Dapper;
using Npgsql;
using formular0.Models;

namespace formular0.Repositories;

public class PlatformRepository : IPlatformRepository
{
    private readonly string _cs;

    public PlatformRepository(string connectionString) => _cs = connectionString;

    private NpgsqlConnection Connect() => new(_cs);

    public List<Platform> GetAll()
    {
        using var conn = Connect();
        return conn.Query<Platform>("SELECT id, name FROM platforms ORDER BY name").AsList();
    }
}

using System.Collections.Generic;
using formular0.Models;

namespace formular0.Repositories;

public interface IGameRepository
{
    List<Game> GetAll();
    void Add(Game game);
    void Update(Game game);
    void Delete(int id);
    decimal GetTotalHours();
    string? GetMostPlayedGame();
}

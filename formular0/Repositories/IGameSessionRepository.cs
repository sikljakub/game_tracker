using System.Collections.Generic;
using formular0.Models;

namespace formular0.Repositories;

public interface IGameSessionRepository
{
    List<GameSession> GetByGame(int gameId);
    void Add(GameSession session);
    void Update(GameSession session);
    void Delete(int id);
}

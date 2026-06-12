using System.Collections.Generic;
using formular0.Models;

namespace formular0.Repositories;

public interface IPlatformRepository
{
    List<Platform> GetAll();
}

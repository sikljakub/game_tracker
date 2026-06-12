using System;

namespace formular0.Models;

public class GameSession
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public DateTime PlayedOn { get; set; } = DateTime.Today;
    public decimal HoursPlayed { get; set; }
    public string Note { get; set; } = "";
}

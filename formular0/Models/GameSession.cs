using System;

namespace formular0.Models;

public class GameSession
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public DateOnly PlayedOn { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public decimal HoursPlayed { get; set; }
    public string Note { get; set; } = "";

    // formátované datum pro zobrazení v UI
    public string PlayedOnFormatted => PlayedOn.ToString("dd.MM.yyyy", null);
}

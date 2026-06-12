namespace formular0.Models;

public class Game
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public int PlatformId { get; set; }
    public string PlatformName { get; set; } = "";
    public int? ReleaseYear { get; set; }
    public string Note { get; set; } = "";
}

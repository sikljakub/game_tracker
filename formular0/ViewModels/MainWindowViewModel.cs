using formular0.Models;
using formular0.Repositories;

namespace formular0.ViewModels;

// Hlavní ViewModel — řídí navigaci mezi stránkami
// ContentControl v MainWindow.axaml zobrazuje vždy to co je v CurrentPage
// ViewLocator automaticky najde správný View pro každý ViewModel
public class MainWindowViewModel : ViewModelBase
{
    // repozitáře předány přes DI (Dependency Injection)
    private readonly IGameRepository _games;
    private readonly IGameSessionRepository _sessions;
    private readonly IPlatformRepository _platforms;

    // aktuálně zobrazená stránka (ViewModel)
    // při změně ViewLocator najde a zobrazí odpovídající View
    private object? _currentPage;
    public object? CurrentPage
    {
        get => _currentPage;
        set => SetField(ref _currentPage, value); // SetField notifikuje UI o změně
    }

    // konstruktor dostane repozitáře z DI kontejneru automaticky
    public MainWindowViewModel(IGameRepository games, IGameSessionRepository sessions, IPlatformRepository platforms)
    {
        _games = games;
        _sessions = sessions;
        _platforms = platforms;
        NavigateToList(); // při startu zobraz seznam her
    }

    // přejde na seznam her
    public void NavigateToList()
        => CurrentPage = new GamesListViewModel(this, _games);

    // přejde na detail konkrétní hry
    public void NavigateToDetail(Game game)
        => CurrentPage = new GameDetailViewModel(this, _sessions, game);

    // přejde na formulář — bez parametru = nová hra, s parametrem = editace
    public void NavigateToForm(Game? game = null)
        => CurrentPage = new GameFormViewModel(this, _games, _platforms, game);
}

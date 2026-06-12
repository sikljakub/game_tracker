using formular0.Models;
using formular0.Repositories;

namespace formular0.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly IGameRepository _games;
    private readonly IGameSessionRepository _sessions;
    private readonly IPlatformRepository _platforms;

    private object? _currentPage;
    public object? CurrentPage
    {
        get => _currentPage;
        set => SetField(ref _currentPage, value);
    }

    public MainWindowViewModel(IGameRepository games, IGameSessionRepository sessions, IPlatformRepository platforms)
    {
        _games = games;
        _sessions = sessions;
        _platforms = platforms;
        NavigateToList();
    }

    public void NavigateToList()
        => CurrentPage = new GamesListViewModel(this, _games);

    public void NavigateToDetail(Game game)
        => CurrentPage = new GameDetailViewModel(this, _sessions, game);

    public void NavigateToForm(Game? game = null)
        => CurrentPage = new GameFormViewModel(this, _games, _platforms, game);
}

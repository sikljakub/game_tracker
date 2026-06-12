using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using formular0.Models;
using formular0.Repositories;

namespace formular0.ViewModels;

public class GameFormViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _nav;
    private readonly IGameRepository _gameRepo;
    private readonly Game? _original;

    public bool IsEdit => _original != null;
    public string FormTitle => IsEdit ? "Upravit hru" : "Přidat hru";

    private string _gameTitle = "";
    public string GameTitle
    {
        get => _gameTitle;
        set => SetField(ref _gameTitle, value);
    }

    private Platform? _selectedPlatform;
    public Platform? SelectedPlatform
    {
        get => _selectedPlatform;
        set => SetField(ref _selectedPlatform, value);
    }

    private string _releaseYear = "";
    public string ReleaseYear
    {
        get => _releaseYear;
        set => SetField(ref _releaseYear, value);
    }

    private string _note = "";
    public string Note
    {
        get => _note;
        set => SetField(ref _note, value);
    }

    private string _error = "";
    public string Error
    {
        get => _error;
        set => SetField(ref _error, value);
    }

    public ObservableCollection<Platform> Platforms { get; } = new();

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public GameFormViewModel(MainWindowViewModel nav, IGameRepository gameRepo,
        IPlatformRepository platformRepo, Game? game = null)
    {
        _nav = nav;
        _gameRepo = gameRepo;
        _original = game;

        foreach (var p in platformRepo.GetAll())
            Platforms.Add(p);

        if (game != null)
        {
            GameTitle = game.Title;
            SelectedPlatform = Platforms.FirstOrDefault(p => p.Id == game.PlatformId);
            ReleaseYear = game.ReleaseYear?.ToString() ?? "";
            Note = game.Note;
        }

        CancelCommand = new RelayCommand(_ => _nav.NavigateToList());

        SaveCommand = new RelayCommand(_ =>
        {
            Error = "";
            if (string.IsNullOrWhiteSpace(GameTitle))
            {
                Error = "Název hry je povinný.";
                return;
            }
            if (SelectedPlatform == null)
            {
                Error = "Vyber platformu.";
                return;
            }

            int? year = null;
            if (!string.IsNullOrWhiteSpace(ReleaseYear))
            {
                if (!int.TryParse(ReleaseYear, out var y) || y < 1970 || y > 2100)
                {
                    Error = "Rok vydání musí být číslo mezi 1970 a 2100.";
                    return;
                }
                year = y;
            }

            if (IsEdit)
            {
                _original!.Title = GameTitle;
                _original.PlatformId = SelectedPlatform.Id;
                _original.ReleaseYear = year;
                _original.Note = Note;
                _gameRepo.Update(_original);
            }
            else
            {
                _gameRepo.Add(new Game
                {
                    Title = GameTitle,
                    PlatformId = SelectedPlatform.Id,
                    ReleaseYear = year,
                    Note = Note
                });
            }

            _nav.NavigateToList();
        });
    }
}

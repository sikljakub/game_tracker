using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using formular0.Models;
using formular0.Repositories;

namespace formular0.ViewModels;

// ViewModel pro formulář přidání/editace hry (GameFormView.axaml)
// Stejný formulář slouží pro přidání nové hry i editaci existující
public class GameFormViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _nav;
    private readonly IGameRepository _gameRepo;

    // pokud editujeme, _original drží původní hru; pokud přidáváme, je null
    private readonly Game? _original;

    // IsEdit = true pokud editujeme existující hru
    public bool IsEdit => _original != null;

    // nadpis formuláře se mění podle toho jestli přidáváme nebo editujeme
    public string FormTitle => IsEdit ? "Upravit hru" : "Přidat hru";

    // --- Pole formuláře ---
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

    private string _releaseYear = ""; // string kvůli TextBoxu — převedeme na int při uložení
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

    private string _error = ""; // chybová zpráva validace
    public string Error
    {
        get => _error;
        set => SetField(ref _error, value);
    }

    // seznam platforem pro ComboBox — načtený z DB
    public ObservableCollection<Platform> Platforms { get; } = new();

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public GameFormViewModel(MainWindowViewModel nav, IGameRepository gameRepo,
        IPlatformRepository platformRepo, Game? game = null)
    {
        _nav = nav;
        _gameRepo = gameRepo;
        _original = game;

        // načte platformy z DB do ComboBoxu
        foreach (var p in platformRepo.GetAll())
            Platforms.Add(p);

        // pokud editujeme — předvyplní formulář hodnotami z existující hry
        if (game != null)
        {
            GameTitle = game.Title;
            // najde platformu v seznamu podle ID — aby byl ComboBox správně vybraný
            SelectedPlatform = Platforms.FirstOrDefault(p => p.Id == game.PlatformId);
            ReleaseYear = game.ReleaseYear?.ToString() ?? "";
            Note = game.Note;
        }

        // zruší formulář a vrátí se na seznam
        CancelCommand = new RelayCommand(_ => _nav.NavigateToList());

        // uloží hru (přidá nebo aktualizuje)
        SaveCommand = new RelayCommand(_ =>
        {
            Error = "";

            // validace — název je povinný
            if (string.IsNullOrWhiteSpace(GameTitle))
            {
                Error = "Název hry je povinný.";
                return;
            }

            // validace — platforma je povinná
            if (SelectedPlatform == null)
            {
                Error = "Vyber platformu.";
                return;
            }

            // validace roku — nepovinný, ale pokud je zadán musí být v rozsahu
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
                // editace — aktualizuje původní objekt a pošle do DB
                _original!.Title = GameTitle;
                _original.PlatformId = SelectedPlatform.Id;
                _original.ReleaseYear = year;
                _original.Note = Note;
                _gameRepo.Update(_original);
            }
            else
            {
                // přidání — vytvoří nový objekt a vloží do DB
                _gameRepo.Add(new Game
                {
                    Title = GameTitle,
                    PlatformId = SelectedPlatform.Id,
                    ReleaseYear = year,
                    Note = Note
                });
            }

            _nav.NavigateToList(); // po uložení se vrátí na seznam
        });
    }
}

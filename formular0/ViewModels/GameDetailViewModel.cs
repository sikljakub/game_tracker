using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using formular0.Models;
using formular0.Repositories;

namespace formular0.ViewModels;

// ViewModel pro detail hry (GameDetailView.axaml)
// Zobrazuje info o hře, statistiky a seznam herních relací s možností CRUD
public class GameDetailViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _nav;
    private readonly IGameSessionRepository _sessionRepo;

    // hra jejíž detail zobrazujeme
    public Game Game { get; }

    // seznam herních relací — ObservableCollection notifikuje UI při změnách
    public ObservableCollection<GameSession> Sessions { get; } = new();

    // --- Formulář pro přidání nové relace ---
    private DateTime? _newPlayedOn = DateTime.Today; // výchozí = dnes
    public DateTime? NewPlayedOn
    {
        get => _newPlayedOn;
        set => SetField(ref _newPlayedOn, value);
    }

    private decimal? _newHours;
    public decimal? NewHours
    {
        get => _newHours;
        set => SetField(ref _newHours, value);
    }

    private string _newNote = "";
    public string NewNote
    {
        get => _newNote;
        set => SetField(ref _newNote, value);
    }

    private string _addError = "";
    public string AddError
    {
        get => _addError;
        set => SetField(ref _addError, value);
    }

    // --- Formulář pro editaci existující relace ---
    private bool _isEditing; // true = zobrazí editační formulář v UI
    public bool IsEditing
    {
        get => _isEditing;
        set => SetField(ref _isEditing, value);
    }

    private int _editId; // ID editované relace (neviditelné v UI)

    private DateTime? _editPlayedOn;
    public DateTime? EditPlayedOn
    {
        get => _editPlayedOn;
        set => SetField(ref _editPlayedOn, value);
    }

    private decimal? _editHours;
    public decimal? EditHours
    {
        get => _editHours;
        set => SetField(ref _editHours, value);
    }

    private string _editNote = "";
    public string EditNote
    {
        get => _editNote;
        set => SetField(ref _editNote, value);
    }

    private string _editError = "";
    public string EditError
    {
        get => _editError;
        set => SetField(ref _editError, value);
    }

    // --- Statistiky hry ---
    private decimal _totalHours;
    public decimal TotalHours { get => _totalHours; set => SetField(ref _totalHours, value); }

    private string _lastPlayed = "–";
    public string LastPlayed { get => _lastPlayed; set => SetField(ref _lastPlayed, value); }

    // --- Příkazy ---
    public ICommand BackCommand { get; }             // zpět na seznam
    public ICommand EditGameCommand { get; }          // na formulář pro editaci hry
    public ICommand AddSessionCommand { get; }        // přidá novou relaci
    public ICommand StartEditSessionCommand { get; }  // zahájí editaci relace
    public ICommand SaveSessionCommand { get; }       // uloží editovanou relaci
    public ICommand DeleteSessionCommand { get; }     // smaže relaci
    public ICommand CancelEditCommand { get; }        // zruší editaci

    public GameDetailViewModel(MainWindowViewModel nav, IGameSessionRepository sessionRepo, Game game)
    {
        _nav = nav;
        _sessionRepo = sessionRepo;
        Game = game;

        BackCommand = new RelayCommand(_ => _nav.NavigateToList());
        EditGameCommand = new RelayCommand(_ => _nav.NavigateToForm(Game));

        // přidá novou herní relaci
        AddSessionCommand = new RelayCommand(_ =>
        {
            AddError = "";
            if (NewPlayedOn == null) { AddError = "Datum je povinné."; return; }
            if (NewHours == null || NewHours <= 0) { AddError = "Počet hodin musí být větší než 0."; return; }
            try
            {
                _sessionRepo.Add(new GameSession
                {
                    GameId = Game.Id,
                    PlayedOn = DateOnly.FromDateTime(NewPlayedOn.Value),
                    HoursPlayed = NewHours.Value,
                    Note = NewNote
                });
                NewPlayedOn = DateTime.Today;
                NewHours = null;
                NewNote = "";
                Refresh();
            }
            catch (Exception ex) { AddError = $"Chyba: {ex.Message}"; }
        });

        // vyplní editační formulář hodnotami vybrané relace a zobrazí ho
        StartEditSessionCommand = new RelayCommand(obj =>
        {
            if (obj is not GameSession s) return;
            _editId = s.Id;
            EditPlayedOn = s.PlayedOn.ToDateTime(TimeOnly.MinValue);
            EditHours = s.HoursPlayed;
            EditNote = s.Note;
            EditError = "";
            IsEditing = true;
        });

        // uloží změny editované relace
        SaveSessionCommand = new RelayCommand(_ =>
        {
            EditError = "";
            if (EditPlayedOn == null) { EditError = "Datum je povinné."; return; }
            if (EditHours == null || EditHours <= 0) { EditError = "Počet hodin musí být větší než 0."; return; }
            try
            {
                _sessionRepo.Update(new GameSession
                {
                    Id = _editId,
                    GameId = Game.Id,
                    PlayedOn = DateOnly.FromDateTime(EditPlayedOn.Value),
                    HoursPlayed = EditHours.Value,
                    Note = EditNote
                });
                IsEditing = false;
                Refresh();
            }
            catch (Exception ex) { EditError = $"Chyba: {ex.Message}"; }
        });

        // smaže relaci
        DeleteSessionCommand = new RelayCommand(obj =>
        {
            if (obj is GameSession s)
            {
                try { _sessionRepo.Delete(s.Id); Refresh(); }
                catch (Exception ex) { AddError = $"Chyba: {ex.Message}"; }
            }
        });

        // skryje editační formulář bez uložení
        CancelEditCommand = new RelayCommand(_ => IsEditing = false);

        Refresh(); // načte data při otevření detailu
    }

    // Přenačte seznam relací a přepočítá statistiky
    private void Refresh()
    {
        try
        {
            Sessions.Clear();
            foreach (var s in _sessionRepo.GetByGame(Game.Id))
                Sessions.Add(s);

            // LINQ agregace přímo nad ObservableCollection
            TotalHours = Sessions.Sum(s => s.HoursPlayed);
            LastPlayed = Sessions.Count > 0
                ? Sessions.Max(s => s.PlayedOn).ToString("dd.MM.yyyy", null)
                : "–";
            AddError = "";
        }
        catch (Exception ex)
        {
            AddError = $"Chyba DB: {ex.Message}";
        }
    }
}

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using formular0.Models;
using formular0.Repositories;

namespace formular0.ViewModels;

public class GameDetailViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _nav;
    private readonly IGameSessionRepository _sessionRepo;

    public Game Game { get; }
    public ObservableCollection<GameSession> Sessions { get; } = new();

    // --- Add session form ---
    private DateTimeOffset? _newPlayedOn = DateTimeOffset.Now;
    public DateTimeOffset? NewPlayedOn
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

    // --- Edit session form ---
    private bool _isEditing;
    public bool IsEditing
    {
        get => _isEditing;
        set => SetField(ref _isEditing, value);
    }

    private int _editId;

    private DateTimeOffset? _editPlayedOn;
    public DateTimeOffset? EditPlayedOn
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

    public ICommand BackCommand { get; }
    public ICommand EditGameCommand { get; }
    public ICommand AddSessionCommand { get; }
    public ICommand StartEditSessionCommand { get; }
    public ICommand SaveSessionCommand { get; }
    public ICommand DeleteSessionCommand { get; }
    public ICommand CancelEditCommand { get; }

    public GameDetailViewModel(MainWindowViewModel nav, IGameSessionRepository sessionRepo, Game game)
    {
        _nav = nav;
        _sessionRepo = sessionRepo;
        Game = game;

        BackCommand = new RelayCommand(_ => _nav.NavigateToList());
        EditGameCommand = new RelayCommand(_ => _nav.NavigateToForm(Game));

        AddSessionCommand = new RelayCommand(_ =>
        {
            AddError = "";
            if (NewPlayedOn == null) { AddError = "Datum je povinné."; return; }
            if (NewHours == null || NewHours <= 0) { AddError = "Počet hodin musí být větší než 0."; return; }

            _sessionRepo.Add(new GameSession
            {
                GameId = Game.Id,
                PlayedOn = NewPlayedOn.Value.DateTime,
                HoursPlayed = NewHours.Value,
                Note = NewNote
            });

            NewPlayedOn = DateTimeOffset.Now;
            NewHours = null;
            NewNote = "";
            Refresh();
        });

        StartEditSessionCommand = new RelayCommand(obj =>
        {
            if (obj is not GameSession s) return;
            _editId = s.Id;
            EditPlayedOn = new DateTimeOffset(s.PlayedOn, TimeSpan.Zero);
            EditHours = (decimal?)s.HoursPlayed;
            EditNote = s.Note;
            EditError = "";
            IsEditing = true;
        });

        SaveSessionCommand = new RelayCommand(_ =>
        {
            EditError = "";
            if (EditPlayedOn == null) { EditError = "Datum je povinné."; return; }
            if (EditHours == null || EditHours <= 0) { EditError = "Počet hodin musí být větší než 0."; return; }

            _sessionRepo.Update(new GameSession
            {
                Id = _editId,
                GameId = Game.Id,
                PlayedOn = EditPlayedOn.Value.DateTime,
                HoursPlayed = EditHours.Value,
                Note = EditNote
            });

            IsEditing = false;
            Refresh();
        });

        DeleteSessionCommand = new RelayCommand(obj =>
        {
            if (obj is GameSession s)
            {
                _sessionRepo.Delete(s.Id);
                Refresh();
            }
        });

        CancelEditCommand = new RelayCommand(_ => IsEditing = false);

        Refresh();
    }

    private decimal _totalHours;
    public decimal TotalHours { get => _totalHours; set => SetField(ref _totalHours, value); }

    private string _lastPlayed = "–";
    public string LastPlayed { get => _lastPlayed; set => SetField(ref _lastPlayed, value); }

    private void Refresh()
    {
        Sessions.Clear();
        foreach (var s in _sessionRepo.GetByGame(Game.Id))
            Sessions.Add(s);

        TotalHours = Sessions.Sum(s => s.HoursPlayed);
        LastPlayed = Sessions.Count > 0
            ? Sessions.Max(s => s.PlayedOn).ToString("dd.MM.yyyy")
            : "–";
    }
}

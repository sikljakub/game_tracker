using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using formular0.Models;
using formular0.Repositories;

namespace formular0.ViewModels;

public class GamesListViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _nav;
    private readonly IGameRepository _repo;

    private List<Game> _allGames = new();
    public ObservableCollection<Game> Games { get; } = new();

    // --- Vyhledávání ---
    private string _searchText = "";
    public string SearchText
    {
        get => _searchText;
        set { SetField(ref _searchText, value); ApplyFilter(); }
    }

    // --- Řazení ---
    public List<string> SortOptions { get; } = new()
    {
        "Název (A–Z)", "Název (Z–A)", "Rok (nejnovější)", "Rok (nejstarší)", "Platforma"
    };

    private string _selectedSort = "Název (A–Z)";
    public string SelectedSort
    {
        get => _selectedSort;
        set { SetField(ref _selectedSort, value); ApplyFilter(); }
    }

    // --- Statistiky ---
    private int _totalGames;
    public int TotalGames { get => _totalGames; set => SetField(ref _totalGames, value); }

    private decimal _totalHours;
    public decimal TotalHours { get => _totalHours; set => SetField(ref _totalHours, value); }

    private string _topGame = "–";
    public string TopGame { get => _topGame; set => SetField(ref _topGame, value); }

    private string _dbError = "";
    public string DbError { get => _dbError; set => SetField(ref _dbError, value); }

    public ICommand AddCommand { get; }
    public ICommand DetailCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand ClearSearchCommand { get; }

    public GamesListViewModel(MainWindowViewModel nav, IGameRepository repo)
    {
        _nav = nav;
        _repo = repo;

        AddCommand = new RelayCommand(_ => _nav.NavigateToForm());

        DetailCommand = new RelayCommand(obj =>
        {
            if (obj is Game game) _nav.NavigateToDetail(game);
        });

        DeleteCommand = new RelayCommand(obj =>
        {
            if (obj is Game game)
            {
                _repo.Delete(game.Id);
                _allGames.Remove(game);
                ApplyFilter();
                UpdateStats();
            }
        });

        ClearSearchCommand = new RelayCommand(_ => SearchText = "");

        Refresh();
    }

    private void Refresh()
    {
        try
        {
            _allGames = _repo.GetAll();
            ApplyFilter();
            UpdateStats();
            DbError = "";
        }
        catch (Exception ex)
        {
            DbError = $"Chyba připojení k databázi: {ex.Message}";
        }
    }

    private void ApplyFilter()
    {
        var query = SearchText.Trim().ToLower();

        var filtered = string.IsNullOrEmpty(query)
            ? _allGames
            : _allGames.Where(g =>
                g.Title.ToLower().Contains(query) ||
                g.PlatformName.ToLower().Contains(query)).ToList();

        filtered = SelectedSort switch
        {
            "Název (Z–A)"       => filtered.OrderByDescending(g => g.Title).ToList(),
            "Rok (nejnovější)"  => filtered.OrderByDescending(g => g.ReleaseYear ?? 0).ToList(),
            "Rok (nejstarší)"   => filtered.OrderBy(g => g.ReleaseYear ?? int.MaxValue).ToList(),
            "Platforma"         => filtered.OrderBy(g => g.PlatformName).ThenBy(g => g.Title).ToList(),
            _                   => filtered.OrderBy(g => g.Title).ToList()
        };

        Games.Clear();
        foreach (var g in filtered)
            Games.Add(g);
    }

    private void UpdateStats()
    {
        TotalGames = _allGames.Count;
        TotalHours = _repo.GetTotalHours();
        var top = _repo.GetMostPlayedGame();
        TopGame = top ?? "–";
    }
}

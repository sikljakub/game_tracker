using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using formular0.Models;
using formular0.Repositories;

namespace formular0.ViewModels;

// ViewModel pro hlavní seznam her (GamesListView.axaml)
public class GamesListViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _nav; // pro navigaci na jiné stránky
    private readonly IGameRepository _repo;

    // kompletní seznam her z DB — slouží jako zdroj pro filtrování
    private List<Game> _allGames = new();

    // seznam her zobrazený v UI — může být filtrovaný/seřazený
    // ObservableCollection = speciální seznam který notifikuje UI při změnách
    public ObservableCollection<Game> Games { get; } = new();

    // --- Vyhledávání ---
    private string _searchText = "";
    public string SearchText
    {
        get => _searchText;
        // při každé změně textu automaticky přefiltruje seznam
        set { SetField(ref _searchText, value); ApplyFilter(); }
    }

    // --- Řazení ---
    // možnosti řazení zobrazené v ComboBoxu
    public List<string> SortOptions { get; } = new()
    {
        "Název (A–Z)", "Název (Z–A)", "Rok (nejnovější)", "Rok (nejstarší)", "Platforma"
    };

    private string _selectedSort = "Název (A–Z)";
    public string SelectedSort
    {
        get => _selectedSort;
        // při změně řazení přefiltruje (a přeseřadí) seznam
        set { SetField(ref _selectedSort, value); ApplyFilter(); }
    }

    // --- Statistiky ---
    private int _totalGames;
    public int TotalGames { get => _totalGames; set => SetField(ref _totalGames, value); }

    private decimal _totalHours;
    public decimal TotalHours { get => _totalHours; set => SetField(ref _totalHours, value); }

    private string _topGame = "–";
    public string TopGame { get => _topGame; set => SetField(ref _topGame, value); }

    // chybová zpráva při problému s DB — zobrazí se místo seznamu
    private string _dbError = "";
    public string DbError { get => _dbError; set => SetField(ref _dbError, value); }

    // --- Příkazy (tlačítka) ---
    public ICommand AddCommand { get; }      // přejde na formulář pro novou hru
    public ICommand DetailCommand { get; }   // přejde na detail vybrané hry
    public ICommand DeleteCommand { get; }   // smaže hru
    public ICommand ClearSearchCommand { get; } // vymaže hledaný text

    public GamesListViewModel(MainWindowViewModel nav, IGameRepository repo)
    {
        _nav = nav;
        _repo = repo;

        // přejde na prázdný formulář pro přidání nové hry
        AddCommand = new RelayCommand(_ => _nav.NavigateToForm());

        // přejde na detail hry — CommandParameter předá vybranou hru
        DetailCommand = new RelayCommand(obj =>
        {
            if (obj is Game game)
            {
                try { _nav.NavigateToDetail(game); }
                catch (Exception ex) { DbError = $"Chyba při otevírání detailu: {ex.Message}"; }
            }
        });

        // smaže hru z DB a odebere ji ze seznamu
        DeleteCommand = new RelayCommand(obj =>
        {
            if (obj is Game game)
            {
                try
                {
                    _repo.Delete(game.Id);
                    _allGames.Remove(game);
                    ApplyFilter();
                    UpdateStats();
                }
                catch (Exception ex) { DbError = $"Chyba při mazání: {ex.Message}"; }
            }
        });

        // vymaže hledaný text → ApplyFilter se zavolá automaticky přes setter
        ClearSearchCommand = new RelayCommand(_ => SearchText = "");

        Refresh(); // při vytvoření načte data z DB
    }

    // Načte čerstvá data z DB
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
            // zobrazí chybu v UI místo pádu aplikace
            DbError = $"Chyba připojení k databázi: {ex.Message}";
        }
    }

    // Filtruje a řadí seznam her podle SearchText a SelectedSort
    private void ApplyFilter()
    {
        var query = SearchText.Trim().ToLower();

        // filtruje podle názvu nebo platformy
        var filtered = string.IsNullOrEmpty(query)
            ? _allGames
            : _allGames.Where(g =>
                g.Title.ToLower().Contains(query) ||
                g.PlatformName.ToLower().Contains(query)).ToList();

        // seřadí podle vybraného kritéria (switch expression = moderní switch)
        filtered = SelectedSort switch
        {
            "Název (Z–A)"       => filtered.OrderByDescending(g => g.Title).ToList(),
            "Rok (nejnovější)"  => filtered.OrderByDescending(g => g.ReleaseYear ?? 0).ToList(),
            "Rok (nejstarší)"   => filtered.OrderBy(g => g.ReleaseYear ?? int.MaxValue).ToList(),
            "Platforma"         => filtered.OrderBy(g => g.PlatformName).ThenBy(g => g.Title).ToList(),
            _                   => filtered.OrderBy(g => g.Title).ToList() // výchozí
        };

        // aktualizuje ObservableCollection — UI se automaticky překreslí
        Games.Clear();
        foreach (var g in filtered)
            Games.Add(g);
    }

    // Přepočítá statistiky z DB
    private void UpdateStats()
    {
        TotalGames = _allGames.Count;
        TotalHours = _repo.GetTotalHours();       // SUM hodin z DB
        var top = _repo.GetMostPlayedGame();       // nejhranější hra z DB
        TopGame = top ?? "–";
    }
}

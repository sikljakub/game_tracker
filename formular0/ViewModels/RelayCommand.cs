using System;
using System.Windows.Input;

namespace formular0.ViewModels;

// RelayCommand implementuje ICommand — propojuje tlačítka v UI s metodami ve ViewModelu
// Použití v AXAML: <Button Command="{Binding DeleteCommand}" CommandParameter="{Binding}"/>
public class RelayCommand : ICommand
{
    // akce která se spustí při kliknutí na tlačítko
    private readonly Action<object?> _execute;

    // volitelná podmínka — pokud vrátí false, tlačítko bude zašedlé (disabled)
    private readonly Func<object?, bool>? _canExecute;

    // konstruktor přijme akci a volitelnou podmínku
    public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    // událost která upozorní UI pokud se změní dostupnost tlačítka
    public event EventHandler? CanExecuteChanged;

    // vrátí true pokud je tlačítko aktivní (pokud není podmínka, vždy true)
    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

    // spustí akci — zavolá se při kliknutí na tlačítko
    public void Execute(object? parameter) => _execute(parameter);

    // ručně upozorní UI že se dostupnost tlačítka mohla změnit
    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}

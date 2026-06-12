using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using formular0.ViewModels;

namespace formular0;

// ViewLocator automaticky najde správný View pro každý ViewModel
// Funguje tak že vezme název ViewModelu a nahradí "ViewModel" za "View"
// Příklad: GamesListViewModel → GamesListView
public class ViewLocator : IDataTemplate
{
    // Build = vytvoří instanci View pro daný ViewModel
    public Control? Build(object? data)
    {
        if (data == null)
            return null;

        // vezme plný název třídy a nahradí "ViewModel" za "View"
        // např. "formular0.ViewModels.GamesListViewModel" → "formular0.Views.GamesListView"
        var name = data.GetType().FullName!.Replace("ViewModel", "View");

        // najde typ View podle názvu
        var type = Type.GetType(name);

        if (type != null)
            return (Control)Activator.CreateInstance(type)!; // vytvoří instanci View

        // pokud View neexistuje, zobrazí chybový text
        return new TextBlock
        {
            Text = "View not found: " + name
        };
    }

    // Match = rozhodne jestli tento template platí pro daný objekt
    // vrací true pouze pro ViewModelBase — tím se zabrání chybám pro jiné typy (string, int...)
    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}

using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace formular0.ViewModels;

// Základní třída pro všechny ViewModely
// Implementuje INotifyPropertyChanged — rozhraní které říká UI "hodnota se změnila, překresli se"
public class ViewModelBase : INotifyPropertyChanged
{
    // Událost která se vyvolá při změně jakékoliv vlastnosti
    public event PropertyChangedEventHandler? PropertyChanged;

    // Vyvolá událost PropertyChanged pro danou vlastnost
    // [CallerMemberName] automaticky doplní název volající vlastnosti — nemusíme ho psát ručně
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    // Pomocná metoda pro nastavení hodnoty vlastnosti
    // ref field = odkaz na privátní pole, value = nová hodnota
    // Vrátí true pokud se hodnota změnila, jinak false
    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        // pokud je hodnota stejná, nic neděláme — zamezí zbytečnému překreslování UI
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(name); // upozorní UI na změnu → UI se překreslí
        return true;
    }
}

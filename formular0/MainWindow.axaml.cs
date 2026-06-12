using Avalonia.Controls;
using formular0.ViewModels;

namespace formular0;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = Services.Get<MainWindowViewModel>();
    }
}

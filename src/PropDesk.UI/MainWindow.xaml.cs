using System.Windows;
using PropDesk.UI.ViewModels;

namespace PropDesk.UI;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}
using System.Windows;
using PropDesk.UI.ViewModels;

namespace PropDesk.UI.Views;

public partial class SettingsWindow : Window
{
    public SettingsWindow(SettingsViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
        vm.CloseAction = () => { DialogResult = true; Close(); };
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => Close();
}
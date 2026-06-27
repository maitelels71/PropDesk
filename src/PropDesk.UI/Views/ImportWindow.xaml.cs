using System.Windows;
using PropDesk.UI.ViewModels;

namespace PropDesk.UI.Views;

public partial class ImportWindow : Window
{
    public ImportWindow(ImportViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
        vm.CloseAction = () => { DialogResult = true; Close(); };
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => Close();
}
using System.Windows;
using PropDesk.UI.ViewModels;

namespace PropDesk.UI.Views;

public partial class PayoutHistoryWindow : Window
{
    public PayoutHistoryWindow(PayoutHistoryViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }
    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
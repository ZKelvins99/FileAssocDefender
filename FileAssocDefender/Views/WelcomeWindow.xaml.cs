using System.Windows;
using Wpf.Ui.Controls;

namespace FileAssocDefender.Views;

public partial class WelcomeWindow : FluentWindow
{
    public WelcomeWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is ViewModels.WelcomeViewModel viewModel)
        {
            viewModel.CloseRequested += (_, result) =>
            {
                DialogResult = result;
                Close();
            };
        }
    }
}

using System.Windows;
using System.Windows.Input;

namespace FileAssocDefender.Views;

public partial class DetailDrawer
{
    public DetailDrawer()
    {
        InitializeComponent();
    }

    private void OnOverlayMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is ViewModels.DetailDrawerViewModel vm)
        {
            vm.CloseCommand.Execute(null);
        }
    }

    private void OnDrawerMouseDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
    }
}

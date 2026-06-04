using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FileAssocDefender.Converters;

public sealed class PageIndexToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not int index || parameter is null)
        {
            return Visibility.Collapsed;
        }

        return int.TryParse(parameter.ToString(), out var pageIndex) && index == pageIndex
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;
}

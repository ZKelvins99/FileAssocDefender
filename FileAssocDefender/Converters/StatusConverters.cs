using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using FileAssocDefender.Models;

namespace FileAssocDefender.Converters;

public sealed class StatusToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not AssociationStatus status)
        {
            return Brushes.Gray;
        }

        return status switch
        {
            AssociationStatus.Healthy => new SolidColorBrush(Color.FromRgb(16, 124, 16)),
            AssociationStatus.Hijacked => new SolidColorBrush(Color.FromRgb(196, 43, 28)),
            AssociationStatus.Unknown => new SolidColorBrush(Color.FromRgb(202, 138, 4)),
            _ => Brushes.Gray
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;
}

public sealed class StatusToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not AssociationStatus status)
        {
            return "未知";
        }

        return status switch
        {
            AssociationStatus.Healthy => "正常",
            AssociationStatus.Hijacked => "已劫持",
            AssociationStatus.Unknown => "未知",
            _ => "未知"
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;
}

public sealed class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;
}

public sealed class InverseBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;
}

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace EdlinSoftware.Safe.Views.Converters;

public class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (targetType != typeof(Visibility))
            throw new NotSupportedException();
        
        if (value is string strValue)
        {
            return string.IsNullOrEmpty(strValue)
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        throw new NotSupportedException();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
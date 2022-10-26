using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace EdlinSoftware.Safe.Views.Converters;

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (targetType != typeof(Visibility))
            throw new NotSupportedException();
        
        if (value is bool boolValue)
        {
            if(parameter is string strParameter)
            {
                if("Invert".Equals(strParameter, StringComparison.OrdinalIgnoreCase))
                {
                    boolValue = !boolValue;
                }
            }

            return boolValue
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
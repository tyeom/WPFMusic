using System;
using System.Windows;
using System.Windows.Data;

namespace Common.Converters;

public class BoolReverseToVisConverter : IValueConverter
{
    #region IValueConverter 구현
    public object Convert(
        object value,
        Type targetType,
        object parameter,
        System.Globalization.CultureInfo culture)
    {
        try
        {
            if ((bool)value)
            {
                return Visibility.Collapsed;
            }
            else
            {
                return Visibility.Visible;
            }
        }
        catch (Exception)
        {
            return Visibility.Visible;
        }
    }

    public object ConvertBack(
        object value,
        Type targetType,
        object parameter,
        System.Globalization.CultureInfo culture)
    {
        try
        {
            if ((Visibility)value == Visibility.Collapsed)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (Exception)
        {
            return true;
        }
    }
    #endregion  // IValueConverter 구현
}

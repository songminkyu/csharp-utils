using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Util.Services
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool flag = (value is bool) ? (bool)value : false;
            bool collapsed = (parameter as string) == "collapsed";
            if (flag) return Visibility.Visible;
            return collapsed ? Visibility.Collapsed : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

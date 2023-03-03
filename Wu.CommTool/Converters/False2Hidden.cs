using System;
using System.Globalization;
using System.Windows;
using Wu.Wpf.Converters;

namespace Wu.CommTool.Converters
{
    /// <summary>
    /// bool转换Visibility false为Collapsed
    /// </summary>
    public class False2Hidden : ValueConverterBase<False2Hidden>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool b = System.Convert.ToBoolean(value);
            if (b)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Hidden;
            }
        }
    }
}

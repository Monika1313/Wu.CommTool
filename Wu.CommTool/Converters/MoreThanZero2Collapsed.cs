using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Wu.Wpf.Converters;

namespace Wu.CommTool.Converters
{
    public class MoreThanZero2Collapsed : ValueConverterBase<MoreThanZero2Collapsed>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && int.TryParse(value.ToString(), out int result))
            {
                if (result == 0)
                {
                    return Visibility.Visible;
                }
            }
            return Visibility.Collapsed;
        }
    }
}

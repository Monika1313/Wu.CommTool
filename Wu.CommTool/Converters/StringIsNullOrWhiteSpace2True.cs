using System;
using System.Globalization;
using Wu.Wpf.Converters;

namespace Wu.CommTool.Converters
{
    /// <summary>
    /// string.IsNullOrWhiteSpace   null或空=不可见
    /// </summary>
    public class StringIsNullOrWhiteSpace2Visibility : ValueConverterBase<StringIsNullOrWhiteSpace2Visibility>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrWhiteSpace(value?.ToString()))
                return true;
            return false;
        }
    }
}

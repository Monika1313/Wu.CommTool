using MQTTnet.Formatter;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace Wu.CommTool.Modules.MqttClient.Converters;

public class MqttVersionConverter : IValueConverter
{
    private static readonly Dictionary<object, string> VersionDisplay = new Dictionary<object, string>
    {
        { MqttProtocolVersion.V310, "MQTT 3.1" },
        { MqttProtocolVersion.V311, "MQTT 3.1.1" },
        { MqttProtocolVersion.V500, "MQTT 5.0" },
        //{ MqttProtocolVersion.Unknown, "未知版本" }
    };

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null) return string.Empty;
        return VersionDisplay.ContainsKey(value) ? VersionDisplay[value] : value.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
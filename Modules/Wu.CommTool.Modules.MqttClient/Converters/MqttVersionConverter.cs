using MQTTnet.Formatter;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace Wu.CommTool.Modules.MqttClient.Converters;

public class MqttnetVersionConverter : IValueConverter
{
    // 定义枚举值到友好显示文本的映射
    private static readonly Dictionary<object, string> VersionDisplayTexts = new()
    {
        { MqttProtocolVersion.V500, "MQTT 5.0" },
        { MqttProtocolVersion.V311, "MQTT 3.1.1" },
        { MqttProtocolVersion.V310, "MQTT 3.1" },
        { MqttProtocolVersion.Unknown, "未知" }
    };

    // 可选：定义版本描述信息
    private static readonly Dictionary<object, string> VersionDescriptions = new()
    {
        { MqttProtocolVersion.V500, "最新版本，支持更多特性" },
        { MqttProtocolVersion.V311, "旧版本，兼容性支持" },
        { MqttProtocolVersion.V310, "旧版本，兼容性支持" },
        { MqttProtocolVersion.Unknown, "未知" }
    };

    /// <summary>
    /// 转换单个枚举值为显示文本
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null) return string.Empty;
        return VersionDisplayTexts.ContainsKey(value) ? VersionDisplayTexts[value] : value.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 定义要隐藏的枚举值
    /// </summary>
    private static readonly HashSet<object> HiddenValues = new HashSet<object>
    {
        MqttProtocolVersion.Unknown  // 隐藏Unknown版本
        // 可以根据需要添加其他要隐藏的版本
        // MqttProtocolVersion.V310  // 如果需要隐藏3.1版本
    };

    /// <summary>
    /// 获取过滤后的枚举值集合（隐藏不需要显示的版本）
    /// </summary>
    public static Array GetFilteredMqttVersions()
    {
        var enumType = typeof(MqttProtocolVersion);
        var allValues = Enum.GetValues(enumType);
        List<object> filteredValues = [];

        foreach (var value in allValues)
        {
            if (!HiddenValues.Contains(value))
            {
                filteredValues.Add(value);
            }
        }

        return filteredValues.OrderByDescending(v => (int)v).ToArray();
    }

    /// <summary>
    /// 获取版本的描述信息
    /// </summary>
    public static string GetVersionDescription(MqttProtocolVersion version)
    {
        return VersionDescriptions.ContainsKey(version) ? VersionDescriptions[version] : string.Empty;
    }

    /// <summary>
    /// 检查版本是否应该显示
    /// </summary>
    public static bool IsVersionVisible(MqttProtocolVersion version)
    {
        return !HiddenValues.Contains(version);
    }
}
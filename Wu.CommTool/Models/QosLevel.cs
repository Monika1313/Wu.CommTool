using System.ComponentModel;
using Wu.CommTool.Converters;
using Wu.Wpf.Converters;

namespace Wu.CommTool.Models
{
    /// <summary>
    /// Mqtt Qos消息质量等级
    /// </summary>
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum QosLevel
    {
        [Description("Qos0")]
        AtLeastOnce = 0,
        [Description("Qos1")]
        AtMostOnce = 1,
        [Description("Qos2")]
        ExactlyOnce = 2

    }
}

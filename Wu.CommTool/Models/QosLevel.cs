using System.ComponentModel;
using Wu.CommTool.Converters;

namespace Wu.CommTool.Models
{
    /// <summary>
    /// Qos
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

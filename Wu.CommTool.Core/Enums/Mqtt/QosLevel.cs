namespace Wu.CommTool.Core.Enums;

/// <summary>
/// Mqtt Qos消息质量等级
/// </summary>
[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum QosLevel
{
    [Description("Qos0")]
    Qos0 = 0,
    [Description("Qos1")]
    Qos1 = 1,
    [Description("Qos2")]
    Qos2 = 2
}

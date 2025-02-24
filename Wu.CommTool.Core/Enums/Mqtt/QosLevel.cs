﻿namespace Wu.CommTool.Core.Enums;

/// <summary>
/// Mqtt Qos消息质量等级
/// </summary>
[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum QosLevel
{
    [Description("Qos0")]
    AtMostOnce = 0,
    [Description("Qos1")]
    AtLeastOnce = 1,
    [Description("Qos2")]
    ExactlyOnce = 2
}

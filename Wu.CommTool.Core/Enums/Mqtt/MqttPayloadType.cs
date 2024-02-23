namespace Wu.CommTool.Core.Enums.Mqtt;

/// <summary>
/// 数据类型
/// </summary>
[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum MqttPayloadType
{
    [Description("纯文本 UTF-8")]
    Plaintext,
    [Description("Hex 16进制")]
    Hex,
    [Description("Base64")]
    Base64,
    [Description("Json")]
    Json,

}

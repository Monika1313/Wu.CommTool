namespace Wu.CommTool.Core.Enums.Mqtt;

/// <summary>
/// 数据类型
/// </summary>
[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum MqttPayloadType
{
    [Description("UTF-8 纯文本")]
    Plaintext,
    [Description("Hex 16进制")]
    Hex,
    /// <summary>
    /// 输入的是 Base64 编码的内容
    /// </summary>
    [Description("Base64")]
    Base64,

    /// <summary>
    /// 将输入编码成Base64字符串后再编码成UTF8字符串
    /// </summary>
    [Description("Base64+UTF8")]
    Base64Utf8,
    /// <summary>
    /// 将输入编码成Base64字符串后再编码成UTF8字符串
    /// </summary>
    [Description("Base64+Base64")]

    Base64Base64,
    [Description("Json")]
    Json,
}

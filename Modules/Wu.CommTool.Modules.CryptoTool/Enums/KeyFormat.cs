namespace Wu.CommTool.Modules.CryptoTool.Enums;

[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum KeyFormat
{
    Hex,
    Text
}

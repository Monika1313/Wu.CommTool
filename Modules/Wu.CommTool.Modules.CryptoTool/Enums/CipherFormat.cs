namespace Wu.CommTool.Modules.CryptoTool.Enums;

[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum CipherFormat
{
    Hex,
    Base64
}

namespace Wu.CommTool.Modules.CryptoTool.Enums;

[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum PaddingMode
{
    PKCS7,
    None
}

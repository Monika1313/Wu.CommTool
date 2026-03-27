namespace Wu.CommTool.Modules.CryptoTool.Enums;

[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum CipherMode
{
    ECB,
    CBC
}

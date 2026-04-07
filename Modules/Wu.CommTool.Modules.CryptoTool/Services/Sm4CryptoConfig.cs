using Wu.CommTool.Modules.CryptoTool.Enums;

namespace Wu.CommTool.Modules.CryptoTool.Services;

public sealed partial class Sm4CryptoConfig : ObservableObject
{
    [ObservableProperty] private string key = string.Empty;

    [ObservableProperty] private string iv = string.Empty;

    [ObservableProperty] private CipherMode cipherMode = CipherMode.ECB;

    [ObservableProperty] private PaddingMode paddingMode = PaddingMode.PKCS7;

    [ObservableProperty] private PlainFormat plainFormat = PlainFormat.Utf8;

    [ObservableProperty] private CipherFormat cipherFormat = CipherFormat.Hex;

    [ObservableProperty] private KeyFormat keyFormat = KeyFormat.Hex;
}

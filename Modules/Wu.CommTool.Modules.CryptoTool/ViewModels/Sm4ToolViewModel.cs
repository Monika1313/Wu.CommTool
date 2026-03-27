namespace Wu.CommTool.Modules.CryptoTool.ViewModels;

public partial class Sm4ToolViewModel : NavigationViewModel
{
    public Sm4ToolViewModel()
    {
    }

    public Sm4ToolViewModel(IContainerProvider provider) : base(provider)
    {
    }

    [ObservableProperty]
    private string inputText = string.Empty;

    [ObservableProperty]
    private string outputText = string.Empty;

    [ObservableProperty]
    private string keyText = "Wu.CommTool";

    [ObservableProperty]
    private string ivText = "";

    public ObservableCollection<string> CipherModes { get; } =
    [
        "ECB",
        "CBC"
    ];

    public string SelectedCipherMode
    {
        get => _selectedCipherMode;
        set
        {
            if (SetProperty(ref _selectedCipherMode, value))
            {
                OnPropertyChanged(nameof(IsIvEnabled));
            }
        }
    }
    private string _selectedCipherMode = "ECB";

    public ObservableCollection<string> PaddingModes { get; } =
    [
        "PKCS7",
        "无填充"
    ];

    public string SelectedPaddingMode { get => _selectedPaddingMode; set => SetProperty(ref _selectedPaddingMode, value); }
    private string _selectedPaddingMode = "PKCS7";

    public ObservableCollection<string> KeyFormats { get; } =
    [
        "十六进制(16字节)",
        "文本(自动)",
    ];

    public string SelectedKeyFormat { get => _selectedKeyFormat; set => SetProperty(ref _selectedKeyFormat, value); }
    private string _selectedKeyFormat = "十六进制(16字节)";

    public ObservableCollection<string> PlainFormats { get; } =
    [
        "UTF-8",
        "十六进制"
    ];

    public string SelectedPlainFormat { get => _selectedPlainFormat; set => SetProperty(ref _selectedPlainFormat, value); }
    private string _selectedPlainFormat = "UTF-8";

    public ObservableCollection<string> CipherFormats { get; } =
    [
        "十六进制",
        "Base64"
    ];

    public string SelectedCipherFormat { get => _selectedCipherFormat; set => SetProperty(ref _selectedCipherFormat, value); }
    private string _selectedCipherFormat = "十六进制";

    public bool IsIvEnabled => !string.Equals(SelectedCipherMode, "ECB", StringComparison.OrdinalIgnoreCase);

    [RelayCommand]
    private void Encrypt()
    {
        if (string.IsNullOrWhiteSpace(InputText))
        {
            HcGrowlExtensions.Warning("请先输入内容。");
            return;
        }

        try
        {
            OutputText = CryptoAlgorithms.EncryptSm4(
                InputText,
                KeyText,
                IvText,
                GetCipherMode(),
                GetPaddingMode(),
                GetPlainFormat(),
                GetCipherFormat(),
                GetKeyFormat());
        }
        catch (Exception ex)
        {
            HcGrowlExtensions.Warning($"加密失败：{ex.Message}");
        }
    }

    [RelayCommand]
    private void Decrypt()
    {
        if (string.IsNullOrWhiteSpace(InputText))
        {
            HcGrowlExtensions.Warning("请先输入内容。");
            return;
        }

        try
        {
            OutputText = CryptoAlgorithms.DecryptSm4(
                InputText,
                KeyText,
                IvText,
                GetCipherMode(),
                GetPaddingMode(),
                GetPlainFormat(),
                GetCipherFormat(),
                GetKeyFormat());
        }
        catch (Exception ex)
        {
            HcGrowlExtensions.Warning($"解密失败：{ex.Message}");
        }
    }

    [RelayCommand]
    private void Swap()
    {
        (InputText, OutputText) = (OutputText, InputText);
    }

    [RelayCommand]
    private void Clear()
    {
        InputText = string.Empty;
        OutputText = string.Empty;
    }

    private CryptoAlgorithms.Sm4CipherMode GetCipherMode()
    {
        return string.Equals(SelectedCipherMode, "ECB", StringComparison.OrdinalIgnoreCase)
            ? CryptoAlgorithms.Sm4CipherMode.Ecb
            : CryptoAlgorithms.Sm4CipherMode.Cbc;
    }

    private CryptoAlgorithms.Sm4PaddingMode GetPaddingMode()
    {
        return string.Equals(SelectedPaddingMode, "无填充", StringComparison.OrdinalIgnoreCase)
            ? CryptoAlgorithms.Sm4PaddingMode.None
            : CryptoAlgorithms.Sm4PaddingMode.Pkcs7;
    }

    private CryptoAlgorithms.Sm4TextFormat GetPlainFormat()
    {
        return string.Equals(SelectedPlainFormat, "十六进制", StringComparison.OrdinalIgnoreCase)
            ? CryptoAlgorithms.Sm4TextFormat.Hex
            : CryptoAlgorithms.Sm4TextFormat.Utf8;
    }

    private CryptoAlgorithms.Sm4TextFormat GetCipherFormat()
    {
        return string.Equals(SelectedCipherFormat, "十六进制", StringComparison.OrdinalIgnoreCase)
            ? CryptoAlgorithms.Sm4TextFormat.Hex
            : CryptoAlgorithms.Sm4TextFormat.Base64;
    }

    private CryptoAlgorithms.Sm4KeyFormat GetKeyFormat()
    {
        return string.Equals(SelectedKeyFormat, "十六进制(16字节)", StringComparison.OrdinalIgnoreCase)
            ? CryptoAlgorithms.Sm4KeyFormat.Hex16
            : CryptoAlgorithms.Sm4KeyFormat.Md5OfText;
    }
}

using Wu.CommTool.Modules.CryptoTool.Enums;

namespace Wu.CommTool.Modules.CryptoTool.ViewModels;

public partial class Sm4ToolViewModel : NavigationViewModel
{
    private readonly string configDirectory = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Configs\CryptoTool");
    private readonly string configExtension = "jsonSM4";
    private bool initFlag;

    public Sm4ToolViewModel()
    {
    }

    public Sm4ToolViewModel(IContainerProvider provider) : base(provider)
    {
    }

    public override void OnNavigatedTo(NavigationContext navigationContext)
    {
        if (initFlag)
        {
            return;
        }

        initFlag = true;
        LoadDefaultConfig();
    }

    [ObservableProperty] private string inputText = string.Empty;

    [ObservableProperty] private string outputText = string.Empty;

    [ObservableProperty] private string keyText = "Wu.CommTool";

    [ObservableProperty]
    private string ivText = "";

    public string CurrentConfigFullName
    {
        get => _currentConfigFullName;
        set
        {
            if (SetProperty(ref _currentConfigFullName, value))
            {
                OnPropertyChanged(nameof(CurrentConfigName));
            }
        }
    }
    private string _currentConfigFullName = string.Empty;

    public string CurrentConfigName => System.IO.Path.GetFileNameWithoutExtension(CurrentConfigFullName);

    public ObservableCollection<CipherMode> CipherModes { get; } =
    [
        CipherMode.ECB,
        CipherMode.CBC
    ];


    [ObservableProperty] CipherMode selectedCipherMode = CipherMode.ECB;

    public ObservableCollection<PaddingMode> PaddingModes { get; } =
    [
        PaddingMode.PKCS7,
        PaddingMode.None
    ];

    [ObservableProperty]
    private PaddingMode selectedPaddingMode = PaddingMode.PKCS7;


    [ObservableProperty]
    private KeyFormat selectedKeyFormat = KeyFormat.Hex;

    public ObservableCollection<PlainFormat> PlainFormats { get; } =
    [
        PlainFormat.Utf8,
        PlainFormat.Hex
    ];

    [ObservableProperty]
    private PlainFormat selectedPlainFormat = PlainFormat.Utf8;

    public ObservableCollection<CipherFormat> CipherFormats { get; } =
    [
        CipherFormat.Hex,
        CipherFormat.Base64
    ];

    [ObservableProperty]
    private CipherFormat selectedCipherFormat = CipherFormat.Hex;

    public bool IsIvEnabled => SelectedCipherMode != CipherMode.ECB;

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

    [RelayCommand]
    private void ImportConfig()
    {
        try
        {
            Wu.Utils.IoUtil.Exists(configDirectory);
            Microsoft.Win32.OpenFileDialog dlg = new()
            {
                Title = "请选择导入配置文件...",
                Filter = $"json files(*.{configExtension})|*.{configExtension}",
                FilterIndex = 1,
                InitialDirectory = configDirectory
            };

            if (dlg.ShowDialog() != true)
            {
                return;
            }

            CurrentConfigFullName = dlg.FileName;
            string json = Core.Common.Utils.ReadJsonFile(dlg.FileName);
            Sm4ToolConfig? config = Newtonsoft.Json.JsonConvert.DeserializeObject<Sm4ToolConfig>(json);
            if (config == null)
            {
                HcGrowlExtensions.Warning("读取配置文件失败。");
                return;
            }

            ApplyConfig(config);
            HcGrowlExtensions.Success($"导入配置：{CurrentConfigName}");
        }
        catch (Exception ex)
        {
            HcGrowlExtensions.Warning($"导入配置失败：{ex.Message}");
        }
    }

    [RelayCommand]
    private void ExportConfig()
    {
        try
        {
            Wu.Utils.IoUtil.Exists(configDirectory);
            Microsoft.Win32.SaveFileDialog sfd = new()
            {
                Title = "请选择导出配置文件...",
                Filter = $"json files(*.{configExtension})|*.{configExtension}",
                FilterIndex = 1,
                FileName = string.IsNullOrWhiteSpace(CurrentConfigName) ? "Default" : CurrentConfigName,
                DefaultExt = configExtension,
                InitialDirectory = configDirectory,
                OverwritePrompt = true,
                AddExtension = true,
            };

            if (sfd.ShowDialog() != true)
            {
                return;
            }

            SaveConfigToFile(sfd.FileName);
            HcGrowlExtensions.Success($"导出配置：{CurrentConfigName}");
        }
        catch (Exception ex)
        {
            HcGrowlExtensions.Warning($"导出配置失败：{ex.Message}");
        }
    }

    private CryptoAlgorithms.Sm4CipherMode GetCipherMode()
    {
        return SelectedCipherMode == CipherMode.ECB
            ? CryptoAlgorithms.Sm4CipherMode.Ecb
            : CryptoAlgorithms.Sm4CipherMode.Cbc;
    }

    private CryptoAlgorithms.Sm4PaddingMode GetPaddingMode()
    {
        return SelectedPaddingMode == PaddingMode.None
            ? CryptoAlgorithms.Sm4PaddingMode.None
            : CryptoAlgorithms.Sm4PaddingMode.Pkcs7;
    }

    private CryptoAlgorithms.Sm4TextFormat GetPlainFormat()
    {
        return SelectedPlainFormat == PlainFormat.Hex
            ? CryptoAlgorithms.Sm4TextFormat.Hex
            : CryptoAlgorithms.Sm4TextFormat.Utf8;
    }

    private CryptoAlgorithms.Sm4TextFormat GetCipherFormat()
    {
        return SelectedCipherFormat == CipherFormat.Hex
            ? CryptoAlgorithms.Sm4TextFormat.Hex
            : CryptoAlgorithms.Sm4TextFormat.Base64;
    }

    private CryptoAlgorithms.Sm4KeyFormat GetKeyFormat()
    {
        return SelectedKeyFormat == KeyFormat.Hex
            ? CryptoAlgorithms.Sm4KeyFormat.Hex16
            : CryptoAlgorithms.Sm4KeyFormat.Md5OfText;
    }

    private void LoadDefaultConfig()
    {
        try
        {
            Wu.Utils.IoUtil.Exists(configDirectory);
            string filePath = System.IO.Path.Combine(configDirectory, $"Default.{configExtension}");
            CurrentConfigFullName = filePath;

            if (!System.IO.File.Exists(filePath))
            {
                SaveConfigToFile(filePath);
                return;
            }

            string json = Core.Common.Utils.ReadJsonFile(filePath);
            Sm4ToolConfig? config = Newtonsoft.Json.JsonConvert.DeserializeObject<Sm4ToolConfig>(json);
            if (config != null)
            {
                ApplyConfig(config);
            }
        }
        catch (Exception ex)
        {
            HcGrowlExtensions.Warning($"读取默认配置失败：{ex.Message}");
        }
    }

    private void SaveConfigToFile(string filePath)
    {
        Sm4ToolConfig config = BuildConfig();
        string json = Newtonsoft.Json.JsonConvert.SerializeObject(config, Newtonsoft.Json.Formatting.Indented);
        Core.Common.Utils.WriteJsonFile(filePath, json);
        CurrentConfigFullName = filePath;
    }

    private Sm4ToolConfig BuildConfig()
    {
        return new Sm4ToolConfig
        {
            KeyText = KeyText,
            IvText = IvText,
            SelectedCipherMode = SelectedCipherMode.ToString(),
            SelectedPaddingMode = SelectedPaddingMode.ToString(),
            SelectedKeyFormat = SelectedKeyFormat.ToString(),
            SelectedPlainFormat = SelectedPlainFormat.ToString(),
            SelectedCipherFormat = SelectedCipherFormat.ToString()
        };
    }

    private void ApplyConfig(Sm4ToolConfig config)
    {
        KeyText = config.KeyText ?? string.Empty;
        IvText = config.IvText ?? string.Empty;

        if (!Enum.TryParse(config.SelectedCipherMode, true, out CipherMode cipherMode))
        {
            cipherMode = CipherMode.CBC;
        }
        SelectedCipherMode = cipherMode;
        PaddingMode paddingMode;
        if (string.Equals(config.SelectedPaddingMode, "无填充", StringComparison.OrdinalIgnoreCase))
        {
            paddingMode = PaddingMode.None;
        }
        else if (!Enum.TryParse(config.SelectedPaddingMode, true, out paddingMode))
        {
            paddingMode = PaddingMode.PKCS7;
        }

        SelectedPaddingMode = paddingMode;

        KeyFormat keyFormat;
        if (string.Equals(config.SelectedKeyFormat, "十六进制(16字节)", StringComparison.OrdinalIgnoreCase))
        {
            keyFormat = KeyFormat.Hex;
        }
        else if (string.Equals(config.SelectedKeyFormat, "文本(自动)", StringComparison.OrdinalIgnoreCase))
        {
            keyFormat = KeyFormat.Text;
        }
        else if (!Enum.TryParse(config.SelectedKeyFormat, true, out keyFormat))
        {
            keyFormat = KeyFormat.Text;
        }

        SelectedKeyFormat = keyFormat;

        PlainFormat plainFormat;
        if (string.Equals(config.SelectedPlainFormat, "UTF-8", StringComparison.OrdinalIgnoreCase))
        {
            plainFormat = PlainFormat.Utf8;
        }
        else if (string.Equals(config.SelectedPlainFormat, "十六进制", StringComparison.OrdinalIgnoreCase))
        {
            plainFormat = PlainFormat.Hex;
        }
        else if (!Enum.TryParse(config.SelectedPlainFormat, true, out plainFormat))
        {
            plainFormat = PlainFormat.Utf8;
        }

        SelectedPlainFormat = plainFormat;

        CipherFormat cipherFormat;
        if (string.Equals(config.SelectedCipherFormat, "十六进制", StringComparison.OrdinalIgnoreCase))
        {
            cipherFormat = CipherFormat.Hex;
        }
        else if (string.Equals(config.SelectedCipherFormat, "Base64", StringComparison.OrdinalIgnoreCase))
        {
            cipherFormat = CipherFormat.Base64;
        }
        else if (!Enum.TryParse(config.SelectedCipherFormat, true, out cipherFormat))
        {
            cipherFormat = CipherFormat.Base64;
        }

        SelectedCipherFormat = cipherFormat;
    }

    private sealed class Sm4ToolConfig
    {
        public string? KeyText { get; set; }

        public string? IvText { get; set; }

        public string? SelectedCipherMode { get; set; }

        public string? SelectedPaddingMode { get; set; }

        public string? SelectedKeyFormat { get; set; }

        public string? SelectedPlainFormat { get; set; }

        public string? SelectedCipherFormat { get; set; }
    }
}

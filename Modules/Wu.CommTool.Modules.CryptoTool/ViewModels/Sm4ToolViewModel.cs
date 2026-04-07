using Newtonsoft.Json;
using Wu.CommTool.Modules.CryptoTool.Enums;
using Wu.CommTool.Modules.CryptoTool.Services;

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



    /// <summary>
    /// 加密设置
    /// </summary>
    [ObservableProperty] Sm4CryptoConfig sm4CryptoConfig = new();

    public bool IsIvEnabled => Sm4CryptoConfig.CipherMode != CipherMode.ECB;

    /// <summary>
    /// 加密
    /// </summary>
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
            OutputText = Sm4Cryptography.Encrypt(InputText, Sm4CryptoConfig);
        }
        catch (Exception ex)
        {
            HcGrowlExtensions.Warning($"加密失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 解密
    /// </summary>
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
            OutputText = Sm4Cryptography.Decrypt(InputText, Sm4CryptoConfig);
        }
        catch (Exception ex)
        {
            HcGrowlExtensions.Warning($"解密失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 交换
    /// </summary>
    [RelayCommand]
    private void Swap()
    {
        (InputText, OutputText) = (OutputText, InputText);
    }

    /// <summary>
    /// 清空
    /// </summary>
    [RelayCommand]
    private void Clear()
    {
        InputText = string.Empty;
        OutputText = string.Empty;
    }

    /// <summary>
    /// 导入配置
    /// </summary>
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

            string json = Core.Common.Utils.ReadJsonFile(dlg.FileName);
            Sm4CryptoConfig? config = JsonConvert.DeserializeObject<Sm4CryptoConfig>(json);
            if (config == null)
            {
                HcGrowlExtensions.Warning("读取配置文件失败。");
                return;
            }

            Sm4CryptoConfig = config;
            CurrentConfigFullName = dlg.FileName;
            HcGrowlExtensions.Success($"导入配置：{CurrentConfigName}");
        }
        catch (Exception ex)
        {
            HcGrowlExtensions.Warning($"导入配置失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 导出配置
    /// </summary>
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

    /// <summary>
    /// 加载默认配置
    /// </summary>
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
            Sm4CryptoConfig? config = JsonConvert.DeserializeObject<Sm4CryptoConfig>(json);
            if (config != null)
            {
                Sm4CryptoConfig = config;
            }
        }
        catch (Exception ex)
        {
            HcGrowlExtensions.Warning($"读取默认配置失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 保存配置到文件
    /// </summary>
    /// <param name="filePath"></param>
    private void SaveConfigToFile(string filePath)
    {
        string json = Newtonsoft.Json.JsonConvert.SerializeObject(CreateConfigCopy(Sm4CryptoConfig), Newtonsoft.Json.Formatting.Indented);
        Core.Common.Utils.WriteJsonFile(filePath, json);
        CurrentConfigFullName = filePath;
    }


    private static Sm4CryptoConfig CreateConfigCopy(Sm4CryptoConfig config)
    {
        return new Sm4CryptoConfig
        {
            Key = config.Key,
            Iv = config.Iv,
            CipherMode = config.CipherMode,
            PaddingMode = config.PaddingMode,
            KeyFormat = config.KeyFormat,
            PlainFormat = config.PlainFormat,
            CipherFormat = config.CipherFormat
        };
    }
}

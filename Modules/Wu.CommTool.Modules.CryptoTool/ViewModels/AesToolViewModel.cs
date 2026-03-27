namespace Wu.CommTool.Modules.CryptoTool.ViewModels;

public partial class AesToolViewModel : NavigationViewModel
{
    public AesToolViewModel()
    {
    }

    public AesToolViewModel(IContainerProvider provider) : base(provider)
    {
    }

    [ObservableProperty]
    private string inputText = string.Empty;

    [ObservableProperty]
    private string outputText = string.Empty;

    [ObservableProperty]
    private string keyText = "Wu.CommTool";

    [ObservableProperty]
    private string ivText = "Wu.CommTool";

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
            OutputText = CryptoAlgorithms.EncryptAes(InputText, KeyText, IvText);
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
            OutputText = CryptoAlgorithms.DecryptAes(InputText, KeyText, IvText);
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
}

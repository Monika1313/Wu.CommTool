namespace Wu.CommTool.Modules.CryptoTool.ViewModels;

public partial class Base64ToolViewModel : NavigationViewModel
{
    public Base64ToolViewModel()
    {
    }

    public Base64ToolViewModel(IContainerProvider provider) : base(provider)
    {
    }

    [ObservableProperty]
    private string inputText = string.Empty;

    [ObservableProperty]
    private string outputText = string.Empty;

    [RelayCommand]
    private void Encrypt()
    {
        if (string.IsNullOrWhiteSpace(InputText))
        {
            HcGrowlExtensions.Warning("«Îœ» ‰»Îƒ⁄»›°£");
            return;
        }

        try
        {
            OutputText = CryptoAlgorithms.Base64Encrypt(InputText);
        }
        catch (Exception ex)
        {
            HcGrowlExtensions.Warning($"±‡¬Î ß∞‹£∫{ex.Message}");
        }
    }

    [RelayCommand]
    private void Decrypt()
    {
        if (string.IsNullOrWhiteSpace(InputText))
        {
            HcGrowlExtensions.Warning("«Îœ» ‰»Îƒ⁄»›°£");
            return;
        }

        try
        {
            OutputText = CryptoAlgorithms.Base64Decrypt(InputText);
        }
        catch (Exception ex)
        {
            HcGrowlExtensions.Warning($"Ω‚¬Î ß∞‹£∫{ex.Message}");
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

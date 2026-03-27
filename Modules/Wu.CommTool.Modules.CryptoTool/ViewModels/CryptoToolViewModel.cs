namespace Wu.CommTool.Modules.CryptoTool.ViewModels;

public partial class CryptoToolViewModel : NavigationViewModel
{
    private readonly IRegionManager regionManager;

    public CryptoToolViewModel()
    {
    }

    public CryptoToolViewModel(IContainerProvider provider, IRegionManager regionManager) : base(provider)
    {
        this.regionManager = regionManager;
        SelectedIndex = 0;
        SelectedMenu = MenuBars.Count > 0 ? MenuBars[0] : null;
    }

    public ObservableCollection<MenuBar> MenuBars { get => _menuBars; set => SetProperty(ref _menuBars, value); }
    private ObservableCollection<MenuBar> _menuBars =
    [
        new MenuBar() { Icon = "Number1", Title = "SM4 加解密", NameSpace = nameof(Sm4ToolView) },
        new MenuBar() { Icon = "Number2", Title = "Base64 编解码", NameSpace = nameof(Base64ToolView) },
        new MenuBar() { Icon = "Number3", Title = "AES 加解密", NameSpace = nameof(AesToolView) },
    ];

    public int SelectedIndex { get => _selectedIndex; set => SetProperty(ref _selectedIndex, value); }
    private int _selectedIndex;

    [ObservableProperty]
    private MenuBar? selectedMenu;

    private bool initFlag;

    public override void OnNavigatedTo(NavigationContext navigationContext)
    {
        if (initFlag)
        {
            return;
        }

        initFlag = true;
        NavigateToMenu(SelectedMenu);
    }

    partial void OnSelectedMenuChanged(MenuBar? value)
    {
        NavigateToMenu(value);
    }

    [RelayCommand]
    private void SelectedIndexChanged(MenuBar? obj)
    {
        NavigateToMenu(obj);
    }

    private void NavigateToMenu(MenuBar? obj)
    {
        if (obj == null || string.IsNullOrWhiteSpace(obj.NameSpace))
        {
            return;
        }

        try
        {
            regionManager.RequestNavigate(PrismRegionNames.CryptoToolsViewRegionName, obj.NameSpace);
        }
        catch
        {
        }
    }
}

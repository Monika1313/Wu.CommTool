namespace Wu.CommTool.Modules.ConvertTools.ViewModels;

public class ConvertToolsViewModel : NavigationViewModel
{
    private readonly IRegionManager regionManager;

    #region 构造函数
    public ConvertToolsViewModel()
    {

    }
    public ConvertToolsViewModel(IContainerProvider provider, IRegionManager regionManager) : base(provider)
    {
        this.regionManager = regionManager;
        ExecuteCommand = new DelegateCommand<string>(Execute);
        SelectedIndexChangedCommand = new DelegateCommand<MenuBar>(SelectedIndexChanged);
    }
    #endregion



    #region 属性
    /// <summary>
    /// ModbusRtu功能菜单
    /// </summary>
    public ObservableCollection<MenuBar> MenuBars { get => _MenuBars; set => SetProperty(ref _MenuBars, value); }
    private ObservableCollection<MenuBar> _MenuBars = new()
        {
            new MenuBar() { Icon = "Number1", Title = "值转换", NameSpace = nameof(ValueConvertView) },
            new MenuBar() { Icon = "Number2", Title = "时间戳工具", NameSpace = nameof(TimestampConvertView) },
        };


    /// <summary>
    /// 选中的标签
    /// </summary>
    public int SelectedIndex { get => _SelectedIndex; set => SetProperty(ref _SelectedIndex, value); }
    private int _SelectedIndex;

    private bool InitFlag = false;
    #endregion


    private void Execute(string obj)
    {
        switch (obj)
        {
            case "Test1": Test1(); break;
            default:
                break;
        }
    }

    private void Test1()
    {
        this.regionManager.RequestNavigate(PrismRegionNames.ConvertToolsViewRegionName, nameof(TimestampConvertView), back =>
        {
            if (back.Error != null)
            {

            }
        });
    }

    /// <summary>
    /// 导航至该页面时
    /// </summary>
    /// <param name="navigationContext"></param>
    public override void OnNavigatedTo(NavigationContext navigationContext)
    {
        //首次导航时, 跳转到初始页面
        if (!InitFlag)
        {
            InitFlag = true;
            this.regionManager.RequestNavigate(PrismRegionNames.ConvertToolsViewRegionName, nameof(ValueConvertView), back =>
            {
                if (back.Error != null)
                {

                }
            });
        }
    }


    /// <summary>
    /// 执行命令
    /// </summary>
    public DelegateCommand<string> ExecuteCommand { get; private set; }

    /// <summary>
    /// 页面切换
    /// </summary>
    public DelegateCommand<MenuBar> SelectedIndexChangedCommand { get; private set; }

    #region 方法
    /// <summary>
    /// 页面切换
    /// </summary>
    /// <param name="obj"></param>
    private void SelectedIndexChanged(MenuBar obj)
    {
        try
        {
            regionManager.RequestNavigate(PrismRegionNames.ConvertToolsViewRegionName, obj.NameSpace);
        }
        catch { }
    }
    #endregion
}

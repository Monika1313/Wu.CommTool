using System.Net.Sockets;
using Wu.CommTool.Modules.ModbusTcp.Views;

namespace Wu.CommTool.Modules.ModbusTcp.ViewModels;

public class ModbusTcpViewModel : NavigationViewModel, IDialogHostAware
{
    #region **************************************** 字段 ****************************************
    private readonly IContainerProvider provider;
    private readonly IDialogHostService dialogHost;
    public string DialogHostName { get; set; }
    #endregion

    public ModbusTcpViewModel() { }
    public ModbusTcpViewModel(IContainerProvider provider, IDialogHostService dialogHost) : base(provider)
    {
        this.provider = provider;
        this.dialogHost = dialogHost;

        ExecuteCommand = new(Execute);
        SaveCommand = new DelegateCommand(Save);
        CancelCommand = new DelegateCommand(Cancel);
    }

    #region **************************************** 属性 ****************************************
    /// <summary>
    /// CurrentDto
    /// </summary>
    public object CurrentDto { get => _CurrentDto; set => SetProperty(ref _CurrentDto, value); }
    private object _CurrentDto = new();

    /// <summary>
    /// 功能菜单
    /// </summary>
    public ObservableCollection<MenuBar> MenuBars { get => _MenuBars; set => SetProperty(ref _MenuBars, value); }
    private ObservableCollection<MenuBar> _MenuBars = new()
        {
            new MenuBar() { Icon = "Number1", Title = "主站Master", NameSpace = nameof(ModbusTcpMasterView) },
            //new MenuBar() { Icon = "Number1", Title = "自定义帧", NameSpace = nameof(ModbusTcpCustomFrameView) },
            //new MenuBar() { Icon = "Number2", Title = "搜索设备", NameSpace = nameof(SearchDeviceView) },
            //new MenuBar() { Icon = "Number3", Title = "数据监控", NameSpace = nameof(DataMonitorView) },
            //new MenuBar() { Icon = "Number4", Title = "自动应答", NameSpace = nameof(AutoResponseView) },
        };
    #endregion


    #region **************************************** 命令 ****************************************
    public DelegateCommand SaveCommand { get; set; }
    public DelegateCommand CancelCommand { get; set; }

    /// <summary>
    /// 执行命令
    /// </summary>
    public DelegateCommand<string> ExecuteCommand { get; private set; }
    #endregion


    #region **************************************** 方法 ****************************************
    public void Execute(string obj)
    {
        switch (obj)
        {
            case "Search": Search(); break;
            case "OpenDialogView": OpenDialogView(); break;
            default: break;
        }
    }

    /// <summary>
    /// 导航至该页面触发
    /// </summary>
    /// <param name="navigationContext"></param>
    public override void OnNavigatedTo(NavigationContext navigationContext)
    {
        //Search();
    }

    /// <summary>
    /// 打开ModbusTcp客户端
    /// </summary>
    public void OnModbusTcpClient()
    {
        TcpClient client = new TcpClient("192.168.1.10", 502);
        //IModbusMaster master = ModbusIpMaster.CreateIp(client);
    }

    /// <summary>
    /// 打开该弹窗时执行
    /// </summary>
    public async void OnDialogOpened(IDialogParameters parameters)
    {
        if (parameters != null && parameters.ContainsKey("Value"))
        {
            //var oldDto = parameters.GetValue<Dto>("Value");
            //var getResult = await employeeService.GetSinglePersonalStorageAsync(oldDto);
            //if(getResult != null && getResult.Status)
            //{
            //    CurrentDto = getResult.Result;
            //}
        }
    }


    /// <summary>
    /// 保存
    /// </summary>
    private void Save()
    {
        if (!DialogHost.IsDialogOpen(DialogHostName))
            return;
        //添加返回的参数
        DialogParameters param = new DialogParameters();
        param.Add("Value", CurrentDto);
        //关闭窗口,并返回参数
        DialogHost.Close(DialogHostName, new DialogResult(ButtonResult.OK, param));
    }

    /// <summary>
    /// 取消
    /// </summary>
    private void Cancel()
    {
        //若窗口处于打开状态则关闭
        if (DialogHost.IsDialogOpen(DialogHostName))
            DialogHost.Close(DialogHostName, new DialogResult(ButtonResult.No));
    }

    /// <summary>
    /// 弹窗
    /// </summary>
    private async void OpenDialogView()
    {
        try
        {
            DialogParameters param = new()
            {
                { "Value", CurrentDto }
            };
            //var dialogResult = await dialogHost.ShowDialog(nameof(DialogView), param, nameof(CurrentView));
        }
        catch (Exception ex)
        {

        }
    }

    /// <summary>
    /// 查询数据
    /// </summary>
    private async void Search()
    {
        try
        {
            UpdateLoading(true);

        }
        catch (Exception ex)
        {
            //aggregator.SendMessage($"{ex.Message}", "Main");
        }
        finally
        {
            UpdateLoading(false);
        }
    }
    #endregion
}

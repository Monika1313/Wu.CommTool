﻿namespace Wu.CommTool.Modules.ModbusRtu.ViewModels.DialogViewModels;

public partial class EditFrameViewModel : NavigationViewModel, IDialogHostAware
{
    #region **************************************** 字段 ****************************************
    private readonly IContainerProvider provider;
    private readonly IDialogHostService dialogHost;
    public string DialogHostName { get; set; }
    #endregion

    #region **************************************** 构造函数 ****************************************
    public EditFrameViewModel() { }
    public EditFrameViewModel(IContainerProvider provider, IDialogHostService dialogHost) : base(provider)
    {
        this.provider = provider;
        this.dialogHost = dialogHost;

        ExecuteCommand = new(Execute);
        ModbusRtuFrameTypes =
        [
            ModbusRtuFrameType._0x01请求帧,
            ModbusRtuFrameType._0x02请求帧,
            ModbusRtuFrameType._0x03请求帧,
            ModbusRtuFrameType._0x04请求帧,
            ModbusRtuFrameType._0x05请求帧,
            ModbusRtuFrameType._0x06请求帧,
        ];
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
    #endregion

    #region **************************************** 属性 ****************************************
    /// <summary>
    /// CurrentDto
    /// </summary>
    public object CurrentDto { get => _CurrentDto; set => SetProperty(ref _CurrentDto, value); }
    private object _CurrentDto = new();

    /// <summary>
    /// 帧生成
    /// </summary>
    public ModbusRtuFrameCreator ModbusRtuFrameCreator { get => _ModbusRtuFrameCreator; set => SetProperty(ref _ModbusRtuFrameCreator, value); }
    private ModbusRtuFrameCreator _ModbusRtuFrameCreator = new();

    /// <summary>
    /// 可生成的帧列表
    /// </summary>
    public ObservableCollection<ModbusRtuFrameType> ModbusRtuFrameTypes { get => _ModbusRtuFrameTypes; set => SetProperty(ref _ModbusRtuFrameTypes, value); }
    private ObservableCollection<ModbusRtuFrameType> _ModbusRtuFrameTypes;

    //public ModbusRtuFunctionCode SelectedCode { get => _SelectedCode; set => SetProperty(ref _SelectedCode, value); }
    //private ModbusRtuFunctionCode _SelectedCode = ModbusRtuFunctionCode._0x03;

    /// <summary>
    /// ModbusRtu帧
    /// </summary>
    public ModbusRtuFrame ModbusRtuFrame { get => _ModbusRtuFrame; set => SetProperty(ref _ModbusRtuFrame, value); }
    private ModbusRtuFrame _ModbusRtuFrame = new();
    #endregion


    #region **************************************** 命令 ****************************************
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
            //case "修改生成帧类型": 
            //    break;
            case "OpenDialogView": OpenDialogView(); break;
            default: break;
        }
    }




    /// <summary>
    /// 保存
    /// </summary>
    [RelayCommand]
    void Save()
    {
        if (!DialogHost.IsDialogOpen(DialogHostName))
            return;
        //添加返回的参数
        DialogParameters param = new DialogParameters();
        param.Add("Value", ModbusRtuFrameCreator.FrameStr);
        //关闭窗口,并返回参数
        DialogHost.Close(DialogHostName, new DialogResult(ButtonResult.OK, param));
    }

    /// <summary>
    /// 取消
    /// </summary>
    [RelayCommand]
    void Cancel()
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

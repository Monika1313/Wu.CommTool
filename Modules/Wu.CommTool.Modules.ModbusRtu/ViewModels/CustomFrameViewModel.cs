namespace Wu.CommTool.Modules.ModbusRtu.ViewModels;

public partial class CustomFrameViewModel : NavigationViewModel, IDialogHostAware
{
    #region **************************************** 字段 ****************************************
    private readonly IContainerProvider provider;
    private readonly IDialogHostService dialogHost;

    public string DialogHostName { get; set; }
    #endregion

    public CustomFrameViewModel() { }
    public CustomFrameViewModel(IContainerProvider provider, IDialogHostService dialogHost, ModbusRtuModel modbusRtuModel) : base(provider)
    {
        this.provider = provider;
        this.dialogHost = dialogHost;
        ModbusRtuModel = modbusRtuModel;
        if (ModbusRtuModel.CustomFrames == null || ModbusRtuModel.CustomFrames.Count == 0)
        {
            ModbusRtuModel.CustomFrames = [new ("01 03 0000 0001 "),
                                                  new ("01 04 0000 0001 "),
                                                  new (""),];
        }

        OpenAnalyzeFrameViewCommand = new DelegateCommand<ModbusRtuMessageData>(OpenAnalyzeFrameView);

        SendCustomFrameCommand = new DelegateCommand<CustomFrame>(SendCustomFrame);
        CreateFrameCommand = new DelegateCommand<CustomFrame>(CreateFrame);
        DeleteLineCommand = new DelegateCommand<CustomFrame>(DeleteLine);

        //更新串口列表
        ModbusRtuModel.GetComPorts();

        if (ModbusRtuModel.CustomFrames.Count == 0)
        {
            ModbusRtuModel.CustomFrames = [new CustomFrame  ("01 03 0000 0001 "),
                                                    new ("01 04 0000 0001 "),
                                                    new (""),];
        }

        GetDefaultConfig();
        RefreshQuickImportList();//读取配置文件夹
    }


    #region **************************************** 属性 ****************************************
    [ObservableProperty] object currentDto = new();

    [ObservableProperty] ModbusRtuModel modbusRtuModel;

    /// <summary>
    /// 抽屉
    /// </summary>
    [ObservableProperty] OpenDrawers _OpenDrawers = new();
    #endregion


    #region **************************************** 命令 ****************************************
    /// <summary>
    /// 打开帧解析界面
    /// </summary>
    public DelegateCommand<ModbusRtuMessageData> OpenAnalyzeFrameViewCommand { get; private set; }

    /// <summary>
    /// 发送自定义帧
    /// </summary>
    public DelegateCommand<CustomFrame> SendCustomFrameCommand { get; private set; }

    /// <summary>
    /// 生成帧命令
    /// </summary>
    public DelegateCommand<CustomFrame> CreateFrameCommand { get; private set; }

    /// <summary>
    /// 删除行命令
    /// </summary>
    public DelegateCommand<CustomFrame> DeleteLineCommand { get; private set; }
    #endregion


    #region **************************************** 方法 ****************************************
    [RelayCommand]
    void Execute(string obj)
    {
        switch (obj)
        {
            case "OpenLeftDrawer": OpenDrawers.LeftDrawer = true; break;//打开左侧抽屉
            case "OpenDialogView": OpenDialogView(); break;             //打开弹窗
            case "SendCustomFrame": ModbusRtuModel.SendCustomFrame(); break;  //发送自定义帧
            case "OpenCom":                                             //打开串口
                ModbusRtuModel.OpenCom();
                OpenDrawers.LeftDrawer = false;                         //关闭左侧抽屉;
                break;
            case "CloseCom":
                ModbusRtuModel.CloseCom();                              //关闭串口
                break;
            case "帧生成器":
                OpenEditFrameView();
                break;
            case "新增行":
                ModbusRtuModel.AddNewCustomFrame();
                //ModbusRtuModel.CustomFrames.Add(new CustomFrame());
                break;
            default: break;
        }
    }

    /// <summary>
    /// 导航至该页面触发
    /// </summary>
    /// <param name="navigationContext"></param>
    public override void OnNavigatedTo(NavigationContext navigationContext)
    {

    }

    /// <summary>
    /// 打开该弹窗时执行
    /// </summary>
    public void OnDialogOpened(IDialogParameters parameters)
    {

    }

    [RelayCommand]
    void Save()
    {
        if (!DialogHost.IsDialogOpen(DialogHostName))
            return;
        //添加返回的参数
        DialogParameters param = new()
        {
            { "Value", CurrentDto }
        };
        //关闭窗口,并返回参数
        DialogHost.Close(DialogHostName, new DialogResult(ButtonResult.OK, param));
    }

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
    private void OpenDialogView()
    {

    }

    /// <summary>
    /// 复制Modbus数据帧
    /// </summary>
    /// <param name="obj"></param>
    [RelayCommand]
    private void CopyModbusRtuFrame(ModbusRtuMessageData obj)
    {
        try
        {
            string xx = string.Empty;
            foreach (var item in obj.MessageSubContents)
            {
                xx += $"{item.Content} ";
            }
            Clipboard.SetDataObject(xx);
        }
        catch (Exception ex)
        {
            HcGrowlExtensions.Warning($"{ex.Message}");
        }
    }

    /// <summary>
    /// 打开解析帧页面
    /// </summary>
    /// <param name="data"></param>
    private async void OpenAnalyzeFrameView(ModbusRtuMessageData data)
    {
        try
        {
            if (data == null || data.ModbusRtuFrame == null)
            {
                return;
            }
            DialogParameters param = new()
            {
                { "Value", data.ModbusRtuFrame },
                { "ModbusByteOrder", ModbusRtuModel.ByteOrder }
            };
            var dialogResult = await dialogHost.ShowDialog(nameof(AnalyzeFrameView), param, nameof(ModbusRtuView));
        }
        catch (Exception ex)
        {
            HcGrowlExtensions.Warning(ex.Message);
        }
    }

    /// <summary>
    /// 打开帧生成器页面
    /// </summary>
    /// <param name="data"></param>
    private async void OpenEditFrameView()
    {
        try
        {
            DialogParameters param = new()
            {
                //{ "Value", data.ModbusRtuFrame }
            };
            var dialogResult = await dialogHost.ShowDialog(nameof(EditFrameView), param, nameof(ModbusRtuView));
            if (dialogResult.Result == ButtonResult.OK)
            {
                var x = dialogResult.Parameters.GetValue<string>("Value");
                if (!string.IsNullOrWhiteSpace(x))
                {
                    ModbusRtuModel.InputMessage = x;
                }
            }
        }
        catch (Exception ex)
        {
            HcGrowlExtensions.Warning($"{ex.Message}");
        }
    }

    /// <summary>
    /// 发送自定义帧
    /// </summary>
    /// <param name="frame"></param>
    private void SendCustomFrame(CustomFrame frame)
    {
        ModbusRtuModel.SendCustomFrame(frame);
    }

    /// <summary>
    /// 生成帧内容
    /// </summary>
    /// <param name="frame"></param>
    private async void CreateFrame(CustomFrame frame)
    {
        try
        {
            DialogParameters param = [];
            var dialogResult = await dialogHost.ShowDialog(nameof(EditFrameView), param, nameof(ModbusRtuView));
            if (dialogResult.Result == ButtonResult.OK)
            {
                var result = dialogResult.Parameters.GetValue<string>("Value");
                if (!string.IsNullOrWhiteSpace(result))
                {
                    frame.Frame = result;
                }
            }
        }
        catch (Exception ex)
        {
            HcGrowlExtensions.Warning($"{ex.Message}");
        }
    }

    /// <summary>
    /// 删除行
    /// </summary>
    /// <param name="frame"></param>
    private void DeleteLine(CustomFrame frame)
    {
        if (ModbusRtuModel.CustomFrames.Count > 1)
        {
            ModbusRtuModel.CustomFrames.Remove(frame);
        }
        else
        {
            ModbusRtuModel.ShowErrorMessage("不能删除最后一行...");
        }
    }

    /// <summary>
    /// 自定义帧 编辑
    /// </summary>
    [RelayCommand]
    public async Task EditCustomFrame(CustomFrame model)
    {
        try
        {
            //添加参数
            DialogParameters param = new();
            if (model != null)
                param.Add("Value", model);

            var dialogResult = await dialogHost.ShowDialog(nameof(EditCustomnFrameView), param, ModbusRtuView.ViewName);

            if (dialogResult.Result == ButtonResult.OK)
            {
                try
                {
                    //从结果中获取数据
                    var resultDto = dialogResult.Parameters.GetValue<CustomFrame>("Value");
                }
                catch (Exception ex)
                {
                    Growl.Error(ex.Message);
                }
            }
        }
        catch (Exception ex)
        {
            Growl.Warning(ex.Message);
        }
    }
    #endregion

    #region 配置文件
    /// <summary>
    /// 配置文件夹路径
    /// </summary>
    private readonly string configDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Configs\ModbusRtuCustomFrameConfig");

    /// <summary>
    /// 配置文件扩展名 Json ModbusRtu
    /// </summary>
    private readonly string configExtension = "jmrc";

    /// <summary>
    /// 当前配置文件名称
    /// </summary>
    public string CurrentConfigName => Path.GetFileNameWithoutExtension(CurrentConfigFullName);

    /// <summary>
    /// 当前配置文件完整路径
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentConfigName))]
    string currentConfigFullName = string.Empty;

    /// <summary>
    /// 配置文件列表
    /// </summary>
    [ObservableProperty] ObservableCollection<ConfigFile> configFiles = [];

    /// <summary>
    /// 导出配置文件
    /// </summary>
    [RelayCommand]
    private void ExportConfig()
    {
        try
        {
            Wu.Utils.IoUtil.Exists(configDirectory);
            SaveFileDialog sfd = new()
            {
                Title = "请选择导出配置文件...",                                              //对话框标题
                Filter = $"json files(*.{configExtension})|*.{configExtension}",    //文件格式过滤器
                FilterIndex = 1,                                                         //默认选中的过滤器
                FileName = "Default",                                           //默认文件名
                DefaultExt = configExtension,                                     //默认扩展名
                InitialDirectory = configDirectory,                //指定初始的目录
                OverwritePrompt = true,                                                  //文件已存在警告
                AddExtension = true,                                                     //若用户省略扩展名将自动添加扩展名
            };
            if (sfd.ShowDialog() != true)
                return;
            CurrentConfigFullName = sfd.FileName;
            //将当前的配置序列化为json字符串
            var content = JsonConvert.SerializeObject(ModbusRtuModel);
            //保存文件
            Core.Common.Utils.WriteJsonFile(sfd.FileName, content);
            HcGrowlExtensions.Success($"导出配置:{CurrentConfigName}");
            RefreshQuickImportList();
        }
        catch (Exception ex)
        {
            HcGrowlExtensions.Warning($"配置导出失败 {ex.Message}");
        }
    }

    /// <summary>
    /// 保存配置文件
    /// </summary>
    [RelayCommand]
    private void SaveConfig()
    {
        try
        {
            //将当前的配置序列化为json字符串
            var content = JsonConvert.SerializeObject(ModbusRtuModel);
            //保存文件
            Core.Common.Utils.WriteJsonFile(CurrentConfigFullName, content);
            HcGrowlExtensions.Success($"保存配置:{CurrentConfigName}");
            //RefreshQuickImportList();
        }
        catch (Exception ex)
        {
            HcGrowlExtensions.Warning($"保存配置失败 {ex.Message}");
        }
    }

    /// <summary>
    /// 导入配置文件
    /// </summary>
    [RelayCommand]
    private void ImportConfig()
    {
        try
        {
            Wu.Utils.IoUtil.Exists(configDirectory);
            //选中配置文件
            OpenFileDialog dlg = new()
            {
                Title = "请选择导入配置文件...",                      //对话框标题
                Filter = $"json files(*.{configExtension})|*.{configExtension}",          //文件格式过滤器
                FilterIndex = 1,                                     //默认选中的过滤器
                InitialDirectory = configDirectory
            };

            if (dlg.ShowDialog() != true)
                return;
            var xx = Core.Common.Utils.ReadJsonFile(dlg.FileName);
            var x = JsonConvert.DeserializeObject<ModbusRtuModel>(xx);
            if (x != null)
                UpdateModbusRtuModelCustomnFrame(x);
            HcGrowlExtensions.Success($"导入配置:{CurrentConfigName}");
        }
        catch (Exception ex)
        {
            HcGrowlExtensions.Warning(ex.Message);
        }
    }

    /// <summary>
    /// 导入配置文件
    /// </summary>
    /// <param name="obj"></param>
    [RelayCommand]
    private void QuickImportConfig(ConfigFile obj)
    {
        try
        {
            var xx = Core.Common.Utils.ReadJsonFile(obj.FullName);//读取文件
            var x = JsonConvert.DeserializeObject<ModbusRtuModel>(xx)!;//反序列化
            if (x == null)
            {
                Growl.Warning("读取配置文件失败");
                return;
            }
            CurrentConfigFullName = obj.FullName;
            var importModel = JsonConvert.DeserializeObject<ModbusRtuModel>(xx)!;
            //ModbusRtuModel = importModel;
            UpdateModbusRtuModelCustomnFrame(importModel);
            HcGrowlExtensions.Success($"导入配置:{CurrentConfigName}");
        }
        catch (Exception ex)
        {
            Growl.Warning($"配置文件导入失败 {ex.Message}");
        }
    }

    /// <summary>
    /// 读取默认配置文件 若无则生成
    /// </summary>
    private void GetDefaultConfig()
    {
        //从默认配置文件中读取配置
        try
        {
            var filePath = Path.Combine(configDirectory, $"Default.{configExtension}"); CurrentConfigFullName = filePath;
            if (File.Exists(filePath))
            {
                var x = JsonConvert.DeserializeObject<ModbusRtuModel>(Core.Common.Utils.ReadJsonFile(filePath));
                if (x != null)
                {
                    ModbusRtuModel = x;
                }
            }
            else
            {
                //文件不存在则生成默认配置 
                ModbusRtuModel = new ModbusRtuModel();
                //在默认文件目录生成默认配置文件
                Wu.Utils.IoUtil.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Configs\ModbusRtuConfig"));
                var content = JsonConvert.SerializeObject(ModbusRtuModel);       //将当前的配置序列化为json字符串
                Core.Common.Utils.WriteJsonFile(filePath, content);                     //保存文件
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"配置文件读取失败:{ex.Message}");
        }
    }

    /// <summary>
    /// 更新快速导入配置列表
    /// </summary>
    [RelayCommand]
    private void RefreshQuickImportList()
    {
        try
        {
            Wu.Utils.IoUtil.Exists(configDirectory);//验证文件夹是否存在
            DirectoryInfo Folder = new(configDirectory);
            var a = Folder.GetFiles().Where(x => x.Extension.ToLower().Equals($".{configExtension}")).Select(item => new ConfigFile(item));
            ConfigFiles.Clear();
            foreach (var item in a)
            {
                ConfigFiles.Add(item);
            }
        }
        catch (Exception ex)
        {
            Growl.Error("读取配置文件夹异常: " + ex.Message);
        }
    }

    /// <summary>
    /// 更新模型
    /// </summary>
    /// <param name="import"></param>
    private void UpdateModbusRtuModelCustomnFrame(ModbusRtuModel import)
    {
        ModbusRtuModel.ComConfig = import.ComConfig;
        ModbusRtuModel.ComConfig.ComPort = ModbusRtuModel.ComPorts.FirstOrDefault(x => x.ComId == import.ComConfig.ComPort.ComId);
     
        ModbusRtuModel.InputMessage = import.InputMessage;
        ModbusRtuModel.CrcMode = import.CrcMode;
        ModbusRtuModel.IsPause = import.IsPause;
        ModbusRtuModel.ByteOrder = import.ByteOrder;
        ModbusRtuModel.CustomFrames = new(import.CustomFrames);

        if (ModbusRtuModel.CustomFrames == null || ModbusRtuModel.CustomFrames.Count == 0)
        {
            ModbusRtuModel.CustomFrames = [new ("01 03 0000 0001 "),
                                                  new ("01 04 0000 0001 "),
                                                  new (""),];
        }
    }
    #endregion
}

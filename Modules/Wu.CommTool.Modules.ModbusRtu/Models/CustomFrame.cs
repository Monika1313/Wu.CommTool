namespace Wu.CommTool.Modules.ModbusRtu.Models;

public partial class CustomFrame : ObservableObject
{
    public ModbusRtuModel Owner { get; private set; }
    public CustomFrame(string frame = "")
    {
        Frame = frame;
    }

    public CustomFrame(ModbusRtuModel owner,string frame = "")
    {
        Owner = owner;
        Frame = frame;
    }

    /// <summary>
    /// 帧
    /// </summary>
    [ObservableProperty]
    string frame = "";

    /// <summary>
    /// 启用定时发送
    /// </summary>
    [ObservableProperty]
    bool enableTimer;

    /// <summary>
    /// 发送间隔 单位毫秒
    /// </summary>
    [ObservableProperty]
    double interval = 1000;


}
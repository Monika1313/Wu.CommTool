namespace Wu.CommTool.Modules.ModbusTcp.Models;

public partial class MtcpCustomFrame : ObservableObject
{
    public MtcpCustomFrame(string frame = "")
    {
        Frame = frame;
    }
    /// <summary>
    /// 帧字符串
    /// </summary>
    [ObservableProperty]
    string frame;
}
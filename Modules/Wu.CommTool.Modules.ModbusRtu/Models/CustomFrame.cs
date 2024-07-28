namespace Wu.CommTool.Modules.ModbusRtu.Models;

public partial class CustomFrame : ObservableObject
{
    public CustomFrame(string frame = "")
    {
        Frame = frame;
    }

    /// <summary>
    /// 帧
    /// </summary>
    [ObservableProperty]
    string _Frame = "";


}
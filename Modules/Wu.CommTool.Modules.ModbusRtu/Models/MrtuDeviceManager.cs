namespace Wu.CommTool.Modules.ModbusRtu.Models;

/// <summary>
/// ModbusRtu设备 管理
/// </summary>
public partial class MrtuDeviceManager : ObservableObject
{
    /// <summary>
    /// ModbusRtu设备列表
    /// </summary>
    [ObservableProperty]
    ObservableCollection<MrtuDevice> mrtuDevices = [];

    /// <summary>
    /// 状态
    /// </summary>
    [ObservableProperty]
    bool status;

    /// <summary>
    /// 先做只有一台设备的通讯,后续再做多台
    /// </summary>
    /// <param name="md"></param>
    private void GetData(MrtuDevice md)
    {
        //先做只有一台设备的通讯
        foreach (var request in md.SendFrames)
        {
            //发送请求帧
            //接收数据
            var response = new ModbusRtuFrame();
            //接收成功,更新数据
            md.AnalyzeResponse(request, response);
        }
    }


    [RelayCommand]
    [property: JsonIgnore]
    private async Task Run()
    {
        Status = true;
    }

    [RelayCommand]
    [property: JsonIgnore]
    private async Task Stop()
    {
        Status = Status;
    }

}

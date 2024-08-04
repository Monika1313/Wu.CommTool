using System.Diagnostics;

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
    [property:JsonIgnore]
    bool status;

   

    [RelayCommand]
    [property: JsonIgnore]
    private async Task Run()
    {
        Status = true;

        //TODO 遍历MrtuDevice,获取需要使用的串口 先完成只开一个串口
        //

        foreach (var request in MrtuDevices[0].RequestFrames)
        {
            Debug.Write(request + "\n");

            ////发送请求帧
            ////接收数据
            //var response = new ModbusRtuFrame();
            ////接收成功,更新数据
            //md.AnalyzeResponse(request, response);
        }
    }

    [RelayCommand]
    [property: JsonIgnore]
    private async Task Stop()
    {
        Status = false;
    }

}

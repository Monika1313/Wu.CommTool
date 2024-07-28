namespace Wu.CommTool.Modules.ModbusRtu.Models;

/// <summary>
/// ModbusRtu设备
/// </summary>
public partial class MrtuDevice : ObservableObject
{
    /// <summary>
    /// 设备名
    /// </summary>
    [ObservableProperty]
    string name = string.Empty;

    /// <summary>
    /// 通讯口 串口号
    /// </summary>
    [ObservableProperty]
    string port;

    /// <summary>
    /// 通讯口
    /// </summary>
    [ObservableProperty]
    [property:JsonIgnore]
    ComPort comPort;

    /// <summary>
    /// 备注
    /// </summary>
    [ObservableProperty]
    string remark;

    /// <summary>
    /// 设备状态
    /// </summary>
    [ObservableProperty]
    [property:JsonIgnore]
    DeviceStatus status;

    

    [RelayCommand]
    [property: JsonIgnore]
    private void AnalyzeDataAddress()
    {
        //Todo
        //将需要读取数据按字节的起始地址进行排序
        //对排序后的列表求并集获取所有需要读取的字节地址区间
        //排序后的多个区间, 若有超过60+字节的则再次拆分(设备厂商不同,有些设备支持最大读取数量不同)
        //根据区间生成请求帧
        //按顺序发送请求帧
        //每接收一帧有效帧就更新一次对应地址的数据
    }


}

public enum DeviceStatus
{
    Offline,
    Online,
    Error,
    Warning,
}

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
    string communicationPort;

    /// <summary>
    /// 通讯口
    /// </summary>
    [ObservableProperty]
    [property: JsonIgnore]
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
    [property: JsonIgnore]
    DeviceStatus status;


    /// <summary>
    /// 对测点进行分析,得到获取所有测点数据需要发送的请求帧
    /// </summary>
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

    /// <summary>
    /// 测点数据列表
    /// </summary>
    [ObservableProperty]
    ObservableCollection<MrtuData> mrtuDatas = [];

    /// <summary>
    /// 发送帧列表
    /// </summary>
    [ObservableProperty]
    ObservableCollection<ModbusRtuFrame> sendFrames = [];

    /// <summary>
    /// 根据发送的帧 对接收帧进行解析 并赋值测点数据
    /// </summary>
    /// <param name="send"></param>
    /// <param name="receive"></param>
    public void AnalyzeResponse(ModbusRtuFrame send,ModbusRtuFrame receive)
    {
        //从发送帧获取功能码+起始地址+数据数量
        //根据得到的信息获取本次接收的数据可以对哪些测点赋值
    }

}

public enum DeviceStatus
{
    Offline,
    Online,
    Error,
    Warning,
}

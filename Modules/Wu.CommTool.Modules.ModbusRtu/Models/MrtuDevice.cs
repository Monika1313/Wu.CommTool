global using Wu.CommTool.Core.Common;
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
    /// 从站地址
    /// </summary>
    [ObservableProperty]
    byte slaveAddr = 1;

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
    /// 字节序
    /// </summary>
    [ObservableProperty]
    ModbusByteOrder modbusByteOrder;

    /// <summary>
    /// 读取数据的请求帧
    /// </summary>
    [JsonIgnore]
    public List<string> RequestFrames = [];

    /// <summary>
    /// 对测点进行分析,得到获取所有测点数据需要发送的请求帧
    /// </summary>
    [RelayCommand]
    [property: JsonIgnore]
    public void AnalyzeDataAddress()
    {
        List<string> frames = [];
        #region 读取保持寄存器 03 功能码
        //将需要读取数据按字节的起始地址进行排序
        //根据寄存器类型生成两组
        List<MrtuData> holdingList = [.. MrtuDatas.Where(x => x.RegisterType == RegisterType.Holding).ToList().OrderBy(x => x.RegisterAddr)];
        //对排序后的列表求并集获取所有需要读取的字节地址区间
        List<Point> holdingPoints = [];//使用Point表示闭区间
        //遍历需要读取的数据
        //当前仅做字和双字的处理  字节、bit等功能后续再完善
        foreach (var item in holdingList)
        {
            // points  [X1,Y1] [X2,Y2] [X3,Y3]
            //由于列表已经是排序过的,故当item的起始地址不在区间列表的最后一个区间上或下一个地址,就需要另起一个区间了
            if (holdingPoints.Count == 0 || holdingPoints.LastOrDefault().Y + 1 < item.RegisterAddr)
            {
                holdingPoints.Add(new Point(item.RegisterAddr, item.RegisterLastWordAddr));
            }
            //该数据的起始地址在最后一个区间但长度大于Y将最后一个区间扩大
            else if (holdingPoints.LastOrDefault().Y < item.RegisterLastWordAddr)
            {
                holdingPoints[^1] = new Point(holdingPoints.LastOrDefault().X, item.RegisterLastWordAddr);
            }
        }

        //根据区间生成请求帧
        foreach (var p in holdingPoints)
        {
            if (p.Y - p.X < 99)
            {
                frames.Add(ModbusUtils.StrCombineCrcCode($"{SlaveAddr:X2}03{(int)p.X:X4}{(int)(p.Y - p.X + 1):X4}"));
            }
            //若有超过100字节的则再次拆分(设备厂商不同,有些设备支持最大读取数量不同)
            else
            {
                var startAddr = (int)p.X;
                //拆分成一帧读62字
                frames.Add(ModbusUtils.StrCombineCrcCode($"{SlaveAddr:X2}03{startAddr:X4}{62:X4}"));
                startAddr += 58;//两帧之间读取的地址重叠4字,可以保证在临界的数据至少在其中一帧是完整的
                while (true)
                {
                    if (p.Y - startAddr < 99)
                    {
                        frames.Add(ModbusUtils.StrCombineCrcCode($"{SlaveAddr:X2}03{startAddr:X4}{(int)(p.Y - startAddr + 1):X4}"));
                        break;//退出循环
                    }
                    else
                    {
                        frames.Add(ModbusUtils.StrCombineCrcCode($"{SlaveAddr:X2}03{startAddr:X4}{62:X4}"));
                    }
                    startAddr += 58;
                }
            }
        }
        #endregion


        #region 读取输入寄存器 04功能码
        //将需要读取数据按字节的起始地址进行排序
        //输入寄存器
        List<MrtuData> inputList = [.. MrtuDatas.Where(x => x.RegisterType == RegisterType.Input).ToList().OrderBy(x => x.RegisterAddr)];
        //对排序后的列表求并集获取所有需要读取的字节地址区间
        List<Point> inputPoints = [];//使用Point表示闭区间
        //遍历需要读取的数据
        //当前仅做字和双字的处理  字节、bit等功能后续再完善
        foreach (var item in inputList)
        {
            // points  [X1,Y1] [X2,Y2] [X3,Y3]
            //由于列表已经是排序过的,故当item的起始地址不在区间列表的最后一个区间上或下一个地址,就需要另起一个区间了
            if (inputPoints.Count == 0 || inputPoints.LastOrDefault().Y + 1 < item.RegisterAddr)
            {
                inputPoints.Add(new Point(item.RegisterAddr, item.RegisterLastWordAddr));
            }
            //该数据的起始地址在最后一个区间但长度大于Y将最后一个区间扩大
            else if (inputPoints.LastOrDefault().Y < item.RegisterLastWordAddr)
            {
                inputPoints[^1] = new Point(inputPoints.LastOrDefault().X, item.RegisterLastWordAddr);
            }
        }

        //根据区间生成请求帧
        foreach (var p in inputPoints)
        {
            if (p.Y - p.X < 99)
            {
                frames.Add(ModbusUtils.StrCombineCrcCode($"{SlaveAddr:X2}04{(int)p.X:X4}{(int)(p.Y - p.X + 1):X4}"));
            }
            //若有超过100字节的则再次拆分(设备厂商不同,有些设备支持最大读取数量不同)
            else
            {
                var startAddr = (int)p.X;
                //拆分成一帧读62字
                frames.Add(ModbusUtils.StrCombineCrcCode($"{SlaveAddr:X2}03{startAddr:X4}{62:X4}"));
                startAddr += 58;//两帧之间读取的地址重叠4字,可以保证在临界的数据至少在其中一帧是完整的
                while (true)
                {
                    if (p.Y - startAddr < 99)
                    {
                        frames.Add(ModbusUtils.StrCombineCrcCode($"{SlaveAddr:X2}04{startAddr:X4}{(int)(p.Y - startAddr + 1):X4}"));
                        break;//退出循环
                    }
                    else
                    {
                        frames.Add(ModbusUtils.StrCombineCrcCode($"{SlaveAddr:X2}04{startAddr:X4}{62:X4}"));
                    }
                    startAddr += 58;
                }
            }
        }
        #endregion

        //赋值帧列表
        RequestFrames = frames;
    }

    /// <summary>
    /// 测点数据列表
    /// </summary>
    [ObservableProperty]
    ObservableCollection<MrtuData> mrtuDatas = [];

    /// <summary>
    /// 根据发送的帧 对接收帧进行解析 并赋值测点数据
    /// </summary>
    /// <param name="send"></param>
    /// <param name="receive"></param>
    public void AnalyzeResponse(ModbusRtuFrame send, ModbusRtuFrame receive)
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

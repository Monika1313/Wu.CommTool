namespace Wu.CommTool.Modules.ModbusRtu.Models;

/// <summary>
/// 串口配置
/// </summary>
public class ComConfig : BindableBase
{

    /// <summary>
    /// Com口
    /// </summary>
    public KeyValuePair<string, string> Port
    {
        get => _Port;
        set
        {
            _Port = value;
            RaisePropertyChanged(nameof(Port));
        }
    }
    private KeyValuePair<string, string> _Port;

    /// <summary>
    /// 波特率
    /// </summary>
    public BaudRate BaudRate { get => _BaudRate; set => SetProperty(ref _BaudRate, value); }
    private BaudRate _BaudRate = BaudRate._9600;

    /// <summary>
    /// 校验
    /// </summary>
    public Parity Parity { get => _Parity; set => SetProperty(ref _Parity, value); }
    private Parity _Parity = Parity.None;

    /// <summary>
    /// 数据位
    /// </summary>
    public int DataBits { get => _DataBits; set => SetProperty(ref _DataBits, value); }
    private int _DataBits = 8;

    /// <summary>
    /// 停止位
    /// </summary>
    public StopBits StopBits { get => _StopBits; set => SetProperty(ref _StopBits, value); }
    private StopBits _StopBits = StopBits.One;

    /// <summary>
    /// 是否已打开
    /// </summary>
    [JsonIgnore]
    public bool IsOpened { get => _IsOpened; set => SetProperty(ref _IsOpened, value); }
    private bool _IsOpened = false;

    /// <summary>
    /// 是否处于接收数据状态
    /// </summary>
    [JsonIgnore]
    public bool IsReceiving { get => _IsReceiving; set => SetProperty(ref _IsReceiving, value); }
    private bool _IsReceiving = false;

    /// <summary>
    /// 是否处于发送数据状态
    /// </summary>
    [JsonIgnore]
    public bool IsSending { get => _IsSending; set => SetProperty(ref _IsSending, value); }
    private bool _IsSending = false;

    /// <summary>
    /// 分包超时时间
    /// </summary>
    public int TimeOut { get => _TimeOut; set => SetProperty(ref _TimeOut, value); }
    private int _TimeOut = 25;

    /// <summary>
    /// 分包最大字节
    /// </summary>
    public int MaxLength { get => _MaxLength; set => SetProperty(ref _MaxLength, value); }
    private int _MaxLength = 10240;


    /// <summary>
    /// 自动搜索设备的间隔 单位ms
    /// </summary>
    public int SearchInterval { get => _SearchInterval; set => SetProperty(ref _SearchInterval, value); }
    private int _SearchInterval = 50;

}

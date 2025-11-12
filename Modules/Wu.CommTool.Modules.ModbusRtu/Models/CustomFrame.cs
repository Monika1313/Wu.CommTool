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
    [ObservableProperty] string frame = "";

    /// <summary>
    /// 最后一次发布消息的时间,用于周期发送使用
    /// </summary>
    [JsonIgnore] public DateTime LastPublish { get; set; } = DateTime.MinValue;

    /// <summary>
    /// 发送间隔 单位毫秒 最小50ms
    /// </summary>
    public int Interval
    {
        get => interval;
        set
        {
            if (value >= 50)
            {
                SetProperty(ref interval, value);
            }
            else
            {
                SetProperty(ref interval, 50);
            }
        }
    }
    private int interval = 1000;

    /// <summary>
    /// 启用定时发送
    /// </summary>
    [ObservableProperty] bool enable;

   
}
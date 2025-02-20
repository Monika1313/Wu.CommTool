namespace Wu.CommTool.Modules.ModbusRtu.Models;

public partial class CustomFrame : ObservableObject
{
    private Task timerTask;

    public ModbusRtuModel Owner { get; private set; }
    public CustomFrame(string frame = "")
    {
        Frame = frame;
    }

    public CustomFrame(ModbusRtuModel owner, string frame = "")
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
    public bool Enable
    {
        get => enable;
        set
        {
            SetProperty(ref enable, value);
            if (value)
            {
                //启动定时发送线程
                timerTask = new Task(TimerTask);
                timerTask.Start();
            }
        }
    }
    private bool enable;

    private async void TimerTask()
    {
        while (Enable)
        {
            //若串口已打开则发送帧, 否则等待
            if (Owner.ComConfig.IsOpened && !string.IsNullOrWhiteSpace(Frame))
            {
                //发送帧
                Owner.SendCustomFrame(this);
            }
            await Task.Delay(Interval);
        }
    }
}
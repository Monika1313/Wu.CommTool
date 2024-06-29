namespace Wu.CommTool.Modules.ConvertTools.ViewModels;

public class TimestampConvertViewModel : NavigationViewModel, IRegionMemberLifetime
{
    protected System.Timers.Timer timer;

    public bool KeepAlive
    {
        get
        {
            try
            {
                timer.Stop();
            }
            catch { }
            return false;
        }
    }

    public TimestampConvertViewModel()
    {
        timer = new System.Timers.Timer(50);
        timer.Elapsed += Timer_Elapsed;

        ExecuteCommand = new(Execute);


        ConvertTime = DateTime.Now;
        ConvertTimestampMs = Time2Timestamp(ConvertTime, TimestampUnit.毫秒);
        ConvertTimestampS = ConvertTimestampMs / 1000;
    }

    private void Timer_Elapsed(object sender, ElapsedEventArgs e)
    {
        try
        {
            timer.Stop();
            //Wu.Wpf.Common.Utils.ExecuteFunBeginInvoke(() => { CurrentTime = DateTime.Now; });
            Wpf.Utils.ExecuteFunBeginInvoke(() => { CurrentTime = DateTime.Now; });
            timer.Start();
        }
        catch (Exception ex)
        {
            
        }
    }


    #region 属性
    /// <summary>
    /// 当前时间
    /// </summary>
    public DateTime CurrentTime
    {
        get => _CurrentTime; set
        {
            SetProperty(ref _CurrentTime, value);
            CurrentTimestampMs = Time2Timestamp(CurrentTime, TimestampUnit.毫秒);
            CurrentTimestamp = CurrentTimestampMs / 1000;
        }
    }
    private DateTime _CurrentTime = DateTime.Now;

    /// <summary>
    /// 当前时间戳 单位s
    /// </summary>
    public long CurrentTimestamp { get => _CurrentTimestamp; set => SetProperty(ref _CurrentTimestamp, value); }
    private long _CurrentTimestamp = Time2Timestamp(DateTime.Now, TimestampUnit.秒);

    /// <summary>
    /// 当前时间戳 单位ms
    /// </summary>
    public long CurrentTimestampMs { get => _CurrentTimestampMs; set => SetProperty(ref _CurrentTimestampMs, value); }
    private long _CurrentTimestampMs = Time2Timestamp(DateTime.Now, TimestampUnit.毫秒);

    /// <summary>
    /// 转换时间戳 秒
    /// </summary>
    public long ConvertTimestampS
    {
        get => _ConvertTimestampS;
        set
        {
            if (_ConvertTimestampS.Equals(value)) return;
            SetProperty(ref _ConvertTimestampS, value);
            SetProperty(ref _ConvertTimestampMs, _ConvertTimestampS * 1000, nameof(ConvertTimestampMs));
            SetProperty(ref _ConvertTime, Timestamp2Time(_ConvertTimestampS, TimestampUnit.秒), nameof(ConvertTime));
        }
    }
    private long _ConvertTimestampS;

    /// <summary>
    /// 转换时间戳 毫秒
    /// </summary>
    public long ConvertTimestampMs
    {
        get => _ConvertTimestampMs;
        set
        {
            if (_ConvertTimestampMs.Equals(value)) return;
            SetProperty(ref _ConvertTimestampMs, value);
            SetProperty(ref _ConvertTime, Timestamp2Time(_ConvertTimestampMs, TimestampUnit.毫秒), nameof(ConvertTime));
            SetProperty(ref _ConvertTimestampS, _ConvertTimestampMs / 1000, nameof(ConvertTimestampS));
        }
    }
    private long _ConvertTimestampMs;

    /// <summary>
    /// 转换时间
    /// </summary>
    public DateTime ConvertTime
    {
        get => _ConvertTime;
        set
        {
            if (_ConvertTime.Equals(value)) return;
            SetProperty(ref _ConvertTime, value);
            SetProperty(ref _ConvertTimestampMs, Time2Timestamp(ConvertTime, TimestampUnit.毫秒), nameof(ConvertTimestampMs));
            SetProperty(ref _ConvertTimestampS, _ConvertTimestampMs / 1000, nameof(ConvertTimestampS));
        }
    }

    private DateTime _ConvertTime;
    #endregion


    #region 命令
    /// <summary>
    /// 执行命令
    /// </summary>
    public DelegateCommand<string> ExecuteCommand { get; private set; }
    #endregion


    public override void OnNavigatedTo(NavigationContext navigationContext)
    {
        try
        {
            timer.Start();
        }
        catch (Exception)
        {

        }
    }

    /// <summary>
    /// 时间转换时间戳
    /// </summary>
    /// <param name="dt"></param>
    /// <param name="unit"></param>
    /// <returns></returns>
    public static long Time2Timestamp(DateTime dt, TimestampUnit unit)
    {
        if (unit.Equals(TimestampUnit.毫秒))
            // 使用当前时间计时周期数 减去 1970年01月01日计时周期数（621355968000000000）除去后面4位计数（后四位计时单位小于毫秒）再取整（去小数点）。
            return (dt.ToUniversalTime().Ticks - 621355968000000000) / 10000;
        else
            return (dt.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
    }

    /// <summary>
    /// 时间戳反转为时间
    /// </summary>
    /// <param name="TimeStamp">时间戳</param>
    /// <returns>返回一个日期时间</returns>
    public static DateTime Timestamp2Time(long TimeStamp, TimestampUnit unit)
    {
        DateTime startTime = new DateTime(1970, 1, 1);
        if (unit == TimestampUnit.毫秒)
        {
            return startTime.AddTicks(TimeStamp * 10000).AddHours(8);
        }
        else
        {
            return startTime.AddTicks(TimeStamp * 10000000).AddHours(8);
        }
    }


    public void Execute(string obj)
    {
        switch (obj)
        {
            case "CopyCurrentTime": Clipboard.SetDataObject(CurrentTime.ToString("yyyy/MM/dd HH:mm:ss")); ; break;
            case "CopyCurrentTimestamp": Clipboard.SetDataObject(CurrentTimestamp.ToString()); ; break;
            case "CopyCurrentTimestampMs": Clipboard.SetDataObject(CurrentTimestampMs.ToString()); ; break;
            case "CopyTime": Clipboard.SetDataObject(ConvertTime.ToString("yyyy/MM/dd HH:mm:ss")); break;
            case "CopyTimestampS": Clipboard.SetDataObject(ConvertTimestampS.ToString()); ; break;
            case "CopyTimestampMs": Clipboard.SetDataObject(ConvertTimestampMs.ToString()); ; break;
            default: break;
        }
    }
}

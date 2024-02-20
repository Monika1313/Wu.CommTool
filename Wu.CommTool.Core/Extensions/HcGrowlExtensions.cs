using HandyControl.Controls;
using HandyControl.Data;

namespace Wu.CommTool.Core.Extensions;

/// <summary>
/// handycontrol Growl扩展方法
/// </summary>
public static class HcGrowlExtensions
{
    //默认的停留时间
    public const int WaitTime = 2;

    public static void Warning(string message, string token = "", int waitTime = WaitTime, bool showDateTime = false)
    {
        Growl.Warning(new GrowlInfo()
        {
            Message = message,
            WaitTime = waitTime,
            Token = token,
            ShowDateTime = showDateTime
        });
    }

    public static void Success(string message, string token = "", int waitTime = WaitTime, bool showDateTime = false)
    {
        Growl.Success(new GrowlInfo()
        {
            Message = message,
            WaitTime = waitTime,
            Token = token,
            ShowDateTime = showDateTime
        });
    }

    public static void Info(string message, string token = "", int waitTime = WaitTime, bool showDateTime = false)
    {
        Growl.Info(new GrowlInfo()
        {
            Message = message,
            WaitTime = waitTime,
            Token = token,
            ShowDateTime = showDateTime
        });
    }
}
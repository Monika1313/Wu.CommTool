using HandyControl.Controls;
using HandyControl.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wu.CommTool.Extensions
{
    /// <summary>
    /// handycontrol Growl扩展方法
    /// </summary>
    public static class HcGrowlExtensions
    {
        public static void Warning(string message, string token = "", int waitTime = 1,bool showDateTime = false)
        {
            Growl.Warning(new GrowlInfo()
            {
                Message = message,
                WaitTime = waitTime,
                Token = token,
                ShowDateTime = showDateTime
            });
        }

        public static void Success(string message, string token = "", int waitTime = 1, bool showDateTime = false)
        {
            Growl.Success(new GrowlInfo()
            {
                Message = message,
                WaitTime = waitTime,
                Token = token,
                ShowDateTime = showDateTime
            });
        }

        public static void Info(string message, string token = "", int waitTime = 1, bool showDateTime = false)
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
}
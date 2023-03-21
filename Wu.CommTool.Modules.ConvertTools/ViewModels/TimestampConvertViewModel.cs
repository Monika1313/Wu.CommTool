using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Wu.CommTool.Modules.ConvertTools.Enums;
using Wu.ViewModels;

namespace Wu.CommTool.Modules.ConvertTools.ViewModels
{
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
                catch{}
                return false;
            }
        }

        public TimestampConvertViewModel()
        {
            timer = new System.Timers.Timer(1000);
            timer.Elapsed += Timer_Elapsed;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                CurrentTime = DateTime.Now;
                //Wu.Wpf.Common.Utils.ExecuteFunBeginInvoke(() => { CurrentTime = DateTime.Now; });
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
                CurrentTimestamp = Convert2TimeStamp(CurrentTime, TimestampUnit.秒);
                CurrentTimestampMs = Convert2TimeStamp(CurrentTime, TimestampUnit.毫秒);
            }
        }
        private DateTime _CurrentTime = DateTime.Now;

        /// <summary>
        /// 当前时间戳 单位s
        /// </summary>
        public long CurrentTimestamp { get => _CurrentTimestamp; set => SetProperty(ref _CurrentTimestamp, value); }
        private long _CurrentTimestamp = Convert2TimeStamp(DateTime.Now, TimestampUnit.秒);

        /// <summary>
        /// 当前时间戳 单位ms
        /// </summary>
        public long CurrentTimestampMs { get => _CurrentTimestampMs; set => SetProperty(ref _CurrentTimestampMs, value); }
        private long _CurrentTimestampMs = Convert2TimeStamp(DateTime.Now, TimestampUnit.毫秒);
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
        public static long Convert2TimeStamp(DateTime dt,TimestampUnit unit)
        {
            if (unit.Equals(TimestampUnit.秒))
                return (dt.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
            else
                return (dt.ToUniversalTime().Ticks - 621355968000000000) / 10000;
        }

        /// <summary>
        /// 取时间戳
        /// </summary>
        /// <param name="AccurateToMilliseconds">精确到毫秒</param>
        /// <returns>返回一个长整数时间戳</returns>
        public static long GetTimeStamp(bool AccurateToMilliseconds = false)
        {
            if (AccurateToMilliseconds)
            {
                // 使用当前时间计时周期数 减去 1970年01月01日计时周期数（621355968000000000）除去（删掉）后面4位计数（后四位计时单位小于毫秒，快到不要不要）再取整（去小数点）。
                //备注：DateTime.Now.ToUniversalTime不能缩写成DateTime.Now.Ticks，会有好几个小时的误差。
                //621355968000000000计算方法 long ticks = (new DateTime(1970, 1, 1, 8, 0, 0)).ToUniversalTime().Ticks;
                return (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000;
            }
            else
            {
                //上面是精确到毫秒，需要在最后除去（10000），这里只精确到秒，只要在10000后面加三个0即可（1秒等于1000毫米）。
                return (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
            }
        }

        /// <summary>
        /// 时间戳反转为时间，有很多中翻转方法，但是，请不要使用字符串（string）进行操作，大家都知道字符串会很慢！
        /// </summary>
        /// <param name="TimeStamp">时间戳</param>
        /// <param name="AccurateToMilliseconds">是否精确到毫秒</param>
        /// <returns>返回一个日期时间</returns>
        public static DateTime GetTime(long TimeStamp, bool AccurateToMilliseconds = false)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)); // 当地时区
            if (AccurateToMilliseconds)
            {
                return startTime.AddTicks(TimeStamp * 10000);
            }
            else
            {
                return startTime.AddTicks(TimeStamp * 10000000);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wu.CommTool.Models
{
    /// <summary>
    /// 应用配置
    /// </summary>
    public static class AppConfig
    {
        /// <summary>
        /// 窗口尺寸
        /// </summary>
        public static int WinWidth { get; set; }
        public static int WinHeight { get; set; }

        /// <summary>
        /// 全屏
        /// </summary>
        public static bool IsFullScream { get; set; }
    }
}

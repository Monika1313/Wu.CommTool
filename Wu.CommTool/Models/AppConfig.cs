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
    [Serializable]
    public class AppConfig
    {
        /// <summary>
        /// 窗口尺寸
        /// </summary>
        public double WinWidth { get; set; }
        public double WinHeight { get; set; }

        /// <summary>
        /// 最大化
        /// </summary>
        public bool IsMaximized { get; set; }

        /// <summary>
        /// 初始页面
        /// </summary>
        public string DefaultView { get; set; } = "ModbusRtuView";




    }
}

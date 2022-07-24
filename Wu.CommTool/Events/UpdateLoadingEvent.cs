using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wu.CommTool.Events
{
    /// <summary>
    /// 数据更新模型
    /// </summary>
    public class UpdateModel
    {
        /// <summary>
        /// 数据更新窗口是否打开
        /// </summary>
        public bool IsOpen { get; set; }
    }

    public class UpdateLoadingEvent : PubSubEvent<UpdateModel>
    {

    }
}

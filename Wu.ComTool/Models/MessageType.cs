using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wu.ComTool.Converters;

namespace Wu.ComTool.Models
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum MessageType : int
    {
        [Description("消息")]
        Info = 0,
        [Description("接收")]
        Receive =1,
        [Description("发送")]
        Send = 2,
        [Description("错误")]
        Error =3
    }
}

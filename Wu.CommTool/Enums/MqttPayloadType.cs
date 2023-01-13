using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wu.Wpf.Converters;

namespace Wu.CommTool.Enums
{
    /// <summary>
    /// 数据类型
    /// </summary>
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum MqttPayloadType : int
    {
        [Description("纯文本 UTF-8")]
        Plaintext,
        [Description("Hex 16进制")]
        Hex,
        [Description("Base64")]
        Base64,
        [Description("Json")]
        Json,
        
    }
}

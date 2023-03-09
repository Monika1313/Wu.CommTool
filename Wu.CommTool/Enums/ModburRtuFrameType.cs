using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wu.CommTool.Enums
{
    /// <summary>
    /// ModbusRtu数据帧类型
    /// </summary>
    public enum ModburRtuFrameType
    {
        校验失败,
        解析失败,
        请求帧0x03,
        应答帧0x03,
        差错帧0x83,
        请求帧0x10,
        应答帧0x10,
        差错帧0x90,

    }
}

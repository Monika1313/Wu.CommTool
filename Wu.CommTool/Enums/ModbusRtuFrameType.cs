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
    public enum ModbusRtuFrameType
    {
        校验失败,
        解析失败,

        _0x01请求帧,
        _0x01响应帧,
        _0x81错误帧,

        _0x02请求帧,
        _0x02响应帧,
        _0x82错误帧,

        _0x03请求帧,
        _0x03响应帧,
        _0x83错误帧,

        _0x04请求帧,
        _0x04响应帧,
        _0x84错误帧,

        _0x05请求帧,
        _0x05响应帧,
        _0x85错误帧,

        _0x06请求帧,
        _0x06响应帧,
        _0x86错误帧,

        _0x0F请求帧,
        _0x0F响应帧,
        _0x8F错误帧,

        _0x10请求帧,
        _0x10响应帧,
        _0x90错误帧,
    }
}

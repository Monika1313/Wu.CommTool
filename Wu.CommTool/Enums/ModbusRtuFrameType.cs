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
        _0x03请求帧,
        _0x03响应帧,
        _0x83错误帧,
        _0x04请求帧,
        _0x04响应帧,
        _0x84错误帧,
        _0x10请求帧,
        _0x10响应帧,
        _0x90错误帧,
    }
}

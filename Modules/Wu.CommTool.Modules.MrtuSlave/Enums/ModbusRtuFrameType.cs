﻿namespace Wu.CommTool.Modules.MrtuSlave.Enums;

/// <summary>
/// ModbusRtu数据帧类型
/// </summary>
[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum ModbusRtuFrameType
{
    校验失败,
    解析失败,

    [Description("0x01请求帧 读线圈")]
    _0x01请求帧,
    [Description("0x01应答帧 读线圈")]
    _0x01响应帧,
    [Description("0x81错误帧 0x01读线圈的响应错误帧")]
    _0x81错误帧,

    [Description("0x02请求帧 读离散量输入")]
    _0x02请求帧,
    [Description("0x02应答帧 读离散量输入")]
    _0x02响应帧,
    [Description("0x82错误帧 0x02读离散量输入的响应错误帧")]
    _0x82错误帧,

    [Description("0x03请求帧 读多个保持寄存器")]
    _0x03请求帧,
    [Description("0x03应答帧 读多个保持寄存器")]
    _0x03响应帧,
    [Description("0x83错误帧 0x03读多个保持寄存器的响应错误帧")]
    _0x83错误帧,

    [Description("0x04请求帧 读多个输入寄存器")]
    _0x04请求帧,
    [Description("0x04应答帧 读多个输入寄存器")]
    _0x04响应帧,
    [Description("0x84错误帧 0x04读多个输入寄存器的响应错误帧")]
    _0x84错误帧,

    [Description("0x05请求帧 写单个线圈")]
    _0x05请求帧,
    [Description("0x05应答帧 写单个线圈")]
    _0x05响应帧,
    [Description("0x85错误帧 0x05写单个线圈的响应错误帧")]
    _0x85错误帧,

    [Description("0x06请求帧 写单个保持寄存器")]
    _0x06请求帧,
    [Description("0x06应答帧 写单个保持寄存器")]
    _0x06响应帧,
    [Description("0x86错误帧 0x06写单个保持寄存器的响应错误帧")]
    _0x86错误帧,

    [Description("0x0F请求帧 写多个线圈")]
    _0x0F请求帧,
    [Description("0x0F应答帧 写多个线圈")]
    _0x0F响应帧,
    [Description("0x8F错误帧 0x0F写多个线圈的响应错误帧")]
    _0x8F错误帧,


    [Description("0x10请求帧 写多个寄存器")]
    _0x10请求帧,
    [Description("0x10应答帧 写多个寄存器")]
    _0x10响应帧,
    [Description("0x10错误帧 0x10写多个寄存器的响应错误帧")]
    _0x90错误帧,

    [Description("0x14请求帧 读文件记录")]
    _0x14请求帧,
    [Description("0x14应答帧 读文件记录")]
    _0x14响应帧,
    [Description("0x14请求帧 0x14读文件记录错误帧")]
    _0x94错误帧,

    [Description("0x15请求帧 写文件记录")]
    _0x15请求帧,
    [Description("0x15应答帧 写文件记录")]
    _0x15响应帧,
    [Description("0x15请求帧 0x15写文件记录错误帧")]
    _0x95错误帧,

    [Description("0x16请求帧 屏蔽写寄存器")]
    _0x16请求帧,
    [Description("0x16应答帧 屏蔽写寄存器")]
    _0x16响应帧,
    [Description("0x16请求帧 0x16屏蔽写寄存器的响应错误帧")]
    _0x96错误帧,

    [Description("0x17请求帧 读/写多个保持寄存器")]
    _0x17请求帧,
    [Description("0x17应答帧 读/写多个保持寄存器")]
    _0x17响应帧,
    [Description("0x17请求帧 0x17读/写多个保持寄存器的响应错误帧")]
    _0x97错误帧,

    [Description("0x2B请求帧 读设备识别码")]
    _0x2B请求帧,
    [Description("0x2B应答帧 读设备识别码")]
    _0x2B响应帧,
    [Description("0x2B请求帧 读设备识别码的响应错误帧")]
    _0xAB错误帧,

}

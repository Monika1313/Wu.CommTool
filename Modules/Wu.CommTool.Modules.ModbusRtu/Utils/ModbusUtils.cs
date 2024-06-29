namespace Wu.CommTool.Modules.ModbusRtu.Utils;

public static class ModbusUtils
{
    /// <summary>
    /// 字符串添加校验码
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string StrCombineCrcCode(string str)
    {
        var code = Wu.Utils.Crc.Crc16Modbus(str.GetBytes());
        //若校验码为0
        if (code.All(x => x == 0))
        {
            return str;
        }
        Array.Reverse(code);
        return $"{str}{BitConverter.ToString(code).Replace("-", string.Empty)}";
    }
}

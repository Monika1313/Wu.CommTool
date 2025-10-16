namespace Wu.CommTool.Modules.ModbusRtu.Utils;

public class UartUtils
{
    #region 字节数组转换字符串
    public static string Bytes2String(byte[] bytes) => BitConverter.ToString(bytes).Replace("-", string.Empty);
    public static string Bytes2String(List<byte> bytes) => Bytes2String(bytes.ToArray());
    #endregion


    /// <summary>
    /// 对字符串进行crc校验  
    /// 若已经是校验过的帧 调用该方法将不进行校验
    /// </summary>
    /// <param name="msg"></param>
    /// <returns></returns>
    public static string GetCrcedStr(string msg)
    {
        string reMsg = msg.RemoveSpace().Replace("-", string.Empty);
        if (reMsg.Length % 2 == 1)
        {
            throw new ArgumentException("字符数量不符, 应为2的整数倍");
        }
        var msg2 = msg.Replace("-", string.Empty).GetBytes();
        var code = Wu.Utils.Crc.Crc16Modbus(msg2);

        //若已经是校验过的则不校验
        if (code.All(x => x == 0))
        {
            return msg;
        }

        Array.Reverse(code);

        var result = $"{msg.Trim()} {Bytes2String(code)}";
        return result;
    }
}

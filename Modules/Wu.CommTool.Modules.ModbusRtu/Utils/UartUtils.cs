namespace Wu.CommTool.Modules.ModbusRtu.Utils;

public class UartUtils
{
    #region 字节数组转换字符串
    public static string Bytes2String(byte[] bytes) => BitConverter.ToString(bytes).Replace("-", string.Empty);
    public static string Bytes2String(List<byte> bytes) => Bytes2String(bytes.ToArray());
    #endregion


}

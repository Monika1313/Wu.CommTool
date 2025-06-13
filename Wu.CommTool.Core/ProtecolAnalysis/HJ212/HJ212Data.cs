namespace Wu.CommTool.Core.ProtecolAnalysis.HJ212;

/// <summary>
/// 根据HJ 212-2017编制
/// </summary>
public class HJ212Data
{
    public DateTime QN { get; set; }            // 报文编号 QN=YYYYMMDDhhmmsszzz
    public StCode ST { get; set; }              // 系统编码 ST
    public string CN { get; set; }              // 命令编号 CN
    public string PW { get; set; }              // 访问密码 PW
    public string MN { get; set; }              // 设备唯一标识 MN
    public byte Flag { get; set; }              // 标志位 Flag
    public string PNUM { get; set; }            // 总包数 PNUM
    public string PNO { get; set; }             // 包号 PNO
    public string CP { get; set; }              // 指令参数



    public Dictionary<string, string> Content { get; set; } // 数据区内容

    // 添加转换后的数据内容
    public Dictionary<string, WaterPollutantCode.PollutantInfo> ConvertedContent { get; set; }

    // 扩展ToString方法
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"SystemType: {ST}");
        sb.AppendLine($"CommandNumber: {CN}");
        sb.AppendLine($"MN: {MN}");
        sb.AppendLine($"Flag: {Flag}");
        sb.AppendLine("原始数据区内容:");
        foreach (var item in Content)
        {
            sb.AppendLine($"  {item.Key} = {item.Value}");
        }
        
        if (ConvertedContent != null)
        {
            sb.AppendLine("转换后数据区内容:");
            foreach (var item in ConvertedContent)
            {
                var info = item.Value;
                sb.AppendLine($"  {info.Name}: {item.Key} = {item.Value}]");
            }
        }
        
        return sb.ToString();
    }
}

/// <summary>
/// 系统编码
/// </summary>
public enum StCode
{
    地表水质量监测 = 21,
    空气质量监测 = 22,
    声环境质量监测 = 23,
    地下水质量监测 = 24,
    土壤质量监测 = 25,
    海水质量监测 = 26,
    挥发性有机物监测 = 27,
    大气环境污染源 = 31,
    地表水体环境污染源 = 32,
    地下水体环境污染源 = 33,
    海洋环境污染源 = 34,
    土壤环境污染源 = 35,
    声环境污染源 = 36,
    振动环境污染源 = 37,
    放射性环境污染源 = 38,
    工地扬尘污染源 = 39,
    电磁环境污染源 = 41,
    烟气排放过程监控 = 51,
    污水排放过程监控 = 52,
    系统交互 = 91,
}

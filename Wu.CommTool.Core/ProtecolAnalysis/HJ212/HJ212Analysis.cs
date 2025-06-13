namespace Wu.CommTool.Core.ProtecolAnalysis.HJ212;

public class HJ212Analysis
{ 
    // 解析HJ212协议数据
    public static HJ212Data Parse(string data)
    {
        // 基本校验
        if (string.IsNullOrWhiteSpace(data))
        {
            throw new ArgumentException("数据不能为空");
        }

        if (!data.Contains("&&") || !data.EndsWith("&&"))
        {
            throw new ArgumentException("数据格式不正确，缺少数据区标识");
        }

        // 分割协议头和协议数据区
        var parts = data.Split(new[] { "&&" }, StringSplitOptions.None);
        if (parts.Length != 2)
        {
            throw new ArgumentException("数据区格式不正确");
        }

        var header = parts[0];
        var content = parts[1].Substring(0, parts[1].Length - 2); // 去掉结尾的&&

        // 解析协议头
        var headerDict = ParseHeader(header);

        // 解析数据区
        var contentDict = ParseContent(content);

        return new HJ212Data
        {
            //ST = headerDict.ContainsKey("ST") ? headerDict["ST"] : null,
            //CN = headerDict.ContainsKey("CN") ? headerDict["CN"] : null,
            //PW = headerDict.ContainsKey("PW") ? headerDict["PW"] : null,
            //MN = headerDict.ContainsKey("MN") ? headerDict["MN"] : null,
            //Flag = headerDict.ContainsKey("Flag") ? headerDict["Flag"] : null,
            Content = contentDict
        };
    }


    // 解析协议头
    private static Dictionary<string, string> ParseHeader(string header)
    {
        var result = new Dictionary<string, string>();
        var items = header.Split(';');

        foreach (var item in items)
        {
            var kv = item.Split('=');
            if (kv.Length == 2)
            {
                result[kv[0]] = kv[1];
            }
        }

        return result;
    }

    // 解析数据区
    private static Dictionary<string, string> ParseContent(string content)
    {
        var result = new Dictionary<string, string>();

        // 数据区格式示例: DataTime=20230101120000;B01-Rtd=36.91;011-Rtd=23.4;011-Flag=N

        // 使用正则表达式匹配键值对
        var matches = Regex.Matches(content, @"([^=;]+)=([^=;]+)");

        foreach (Match match in matches)
        {
            if (match.Groups.Count == 3)
            {
                result[match.Groups[1].Value] = match.Groups[2].Value;
            }
        }

        return result;
    }


}

namespace Wu.CommTool.Modules.ModbusRtu.Models;

public partial class ModbusRtuAutoResponseData : ObservableObject
{
    /// <summary>
    /// 名称
    /// </summary>
    [ObservableProperty] string name = string.Empty;

    /// <summary>
    /// 优先级
    /// </summary>
    [ObservableProperty] int priority;

    /// <summary>
    /// 匹配模板
    /// </summary>
    [ObservableProperty] string mateTemplate = string.Empty;

    /// <summary>
    /// 应答模板
    /// </summary>
    [ObservableProperty] string responseTemplate = string.Empty;

    /// <summary>
    /// 匹配模板是否为正则表达式
    /// </summary>
    [ObservableProperty] bool isRegular = false;

    /// <summary>
    /// 是否启用
    /// </summary>
    [ObservableProperty] bool isEnable = true;

    ///// <summary>
    ///// 是否自动添加CRC校验
    ///// </summary>
    //[ObservableProperty] bool autoCrc = true;
}

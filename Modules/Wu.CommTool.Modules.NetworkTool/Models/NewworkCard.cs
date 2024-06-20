namespace Wu.CommTool.Modules.NetworkTool.Models;

/// <summary>
/// 网卡
/// </summary>
public partial class NetworkCard : ObservableObject
{
    readonly ManagementObject mo;

    public NetworkCard(ManagementObject mo)
    {
        this.mo = mo;
        UpdateInfo();
    }

    public void UpdateInfo()
    {
        try
        {

            // 获取适配器的描述
            string description = mo["Description"]?.ToString();
            string x1 = mo["PhysicalAdapter"]?.ToString();//是否为物理网卡
            Enable = Convert.ToBoolean(mo["NetEnabled"] ?? false);//网卡启用禁用状态
            ConnectionId = mo["NetConnectionID"]?.ToString();//连接名称
            Name = mo["Name"]?.ToString();                   //驱动程序
            Manufacturer = mo["Manufacturer"]?.ToString();   //制造商

            #region 遍历mo每个属性
            List<string> strings = new List<string>();
            foreach (PropertyData property in mo.Properties)
            {
                strings.Add(property.Name);
                var name = property.Name;
                // 检查属性值是否为空
                var value = property.Value ?? "null";
                // 如果属性是一个数组，将其转换为字符串
                if (value is Array)
                {
                    value = string.Join(", ", (Array)value);
                }
            }
            #endregion
        }
        catch (Exception ex)
        {

        }
    }



    [ObservableProperty]
    string name;
    
    /// <summary>
    /// 网卡启用禁用状态
    /// </summary>
    [ObservableProperty]
    bool enable;

    [ObservableProperty]
    bool dhcpEnable;

    [ObservableProperty]
    string connectionId;

    [ObservableProperty]
    string manufacturer;
}

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


    public void GetWin32_NetworkAdapterConfiguration()
    {
        // 获取适配器的设备ID
        string deviceId = mo["DeviceID"].ToString();

        // 查询对应的网络适配器配置
        ManagementObjectSearcher configSearcher = new ManagementObjectSearcher(
            $"SELECT * FROM Win32_NetworkAdapterConfiguration WHERE SettingID = '{deviceId}'");
        var x = configSearcher.Get().Count;
        foreach (ManagementObject config in configSearcher.Get())
        {
            // 检查是否包含DHCPEnabled属性并获取其值
            if (config.Properties["DHCPEnabled"] != null)
            {
                DhcpEnable = (bool)config["DHCPEnabled"];

                // 获取适配器的描述
                string description = (string)config["Description"];
            }
        }
    }

    public void UpdateInfo()
    {
        try
        {
            #region 遍历mo每个属性
            //List<string> strings = new List<string>();
            //Dictionary<string,object> dict = new Dictionary<string,object>();
            //foreach (PropertyData property in mo.Properties)
            //{
            //    strings.Add(property.Name);
            //    var name = property.Name;

            //    // 检查属性值是否为空
            //    var value = property.Value ?? "null";
            //    dict.Add(name, value);
            //    // 如果属性是一个数组，将其转换为字符串
            //    if (value is Array)
            //    {
            //        value = string.Join(", ", (Array)value);
            //    }
            //}
            #endregion

            // 获取适配器的描述
            string description = mo["Description"]?.ToString();
            //string x1 = mo["PhysicalAdapter"]?.ToString();//是否为物理网卡
            Enable = Convert.ToBoolean(mo["NetEnabled"] ?? false);//网卡启用禁用状态
            NetConnectionId = mo["NetConnectionID"]?.ToString();//连接名称
            Name = mo["Name"]?.ToString();                   //驱动程序
            Manufacturer = mo["Manufacturer"]?.ToString();   //制造商

            //查询该网卡的Win32_NetworkAdapterConfiguration信息,以获取DHCP状态等
            ManagementObjectSearcher searcherConfig = new("SELECT * FROM Win32_NetworkAdapterConfiguration WHERE Index = " + mo["Index"]);
            foreach (ManagementObject queryObjConfig in searcherConfig.Get())
            {
                DhcpEnable = Convert.ToBoolean(queryObjConfig["DHCPEnabled"]);//网卡DHCP状态
            }
        }
        catch (Exception)
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

    /// <summary>
    /// DHCP状态
    /// </summary>
    [ObservableProperty]
    bool dhcpEnable;

    /// <summary>
    /// 名称
    /// </summary>
    [ObservableProperty]
    string netConnectionId;

    /// <summary>
    /// 制造商
    /// </summary>
    [ObservableProperty]
    string manufacturer;
}

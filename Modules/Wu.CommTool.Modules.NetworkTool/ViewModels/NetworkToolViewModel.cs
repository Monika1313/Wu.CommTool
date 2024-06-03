using Prism.Commands;

namespace Wu.CommTool.Modules.NetworkTool.ViewModels;

public class NetworkToolViewModel : NavigationViewModel
{
    public NetworkToolViewModel()
    {
        ExecuteCommand = new DelegateCommand<string>(Execute);
    }

    private void Execute(string obj)
    {
        switch (obj)
        {
            case "测试": 获取物理网卡信息(); break;
        }
    }


    public DelegateCommand<string> ExecuteCommand { get; private set; }

    void 获取物理网卡信息()
    {
        string query = @"SELECT * FROM Win32_NetworkAdapter WHERE Manufacturer!='Microsoft' AND NOT PNPDeviceID LIKE 'ROOT\\%'";
        ManagementObjectSearcher mos = new(query);
        ManagementObjectCollection moc = mos.Get();
        foreach (ManagementObject mo in moc)
        {
            bool enabled = Convert.ToBoolean(mo["NetEnabled"] ?? false);
            string connectionId = mo["NetConnectionID"]?.ToString();
            string name = mo["Name"]?.ToString();
            string manufacturer = mo["Manufacturer"]?.ToString();

            Console.WriteLine("-----------------------");
            Console.WriteLine($"连接名称：{connectionId}");
            Console.WriteLine($"驱动程序：{name}");
            Console.WriteLine($"启用状态：{enabled}");
            Console.WriteLine($"制造商：{manufacturer}");
        }
    }
}

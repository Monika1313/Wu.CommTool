namespace Wu.CommTool.Modules.ModbusRtu.Models;

/// <summary>
/// ModbusRtu设备 管理
/// </summary>
public partial class MrtuDeviceManager : ObservableObject
{
    /// <summary>
    /// ModbusRtu设备列表
    /// </summary>
    [ObservableProperty]
    ObservableCollection<MrtuDevice> mrtuDevices = [];

    /// <summary>
    /// 状态
    /// </summary>
    [ObservableProperty]
    [property: JsonIgnore]
    bool status;



    [RelayCommand]
    [property: JsonIgnore]
    private async Task Run()
    {
        Status = true;

        //TODO 遍历MrtuDevice,获取需要使用的串口
        foreach (var device in MrtuDevices)
        {
            if (!string.IsNullOrWhiteSpace(device.CommunicationPort))
            {
                //判断是否已存在,已存在则不创建
                //if (ComTaskDict.FindFirst(x => x.Key == device.CommunicationPort).Key == null)
                //{

                //}
                Task task = new(() => ComTask(device.CommunicationPort));
                ComTaskDict.Add(device.CommunicationPort, task);
                //task.Start();
            }
        }


        foreach (var request in MrtuDevices[0].RequestFrames)
        {
            Debug.Write(request + "\n");

            ////发送请求帧
            ////接收数据
            //var response = new ModbusRtuFrame();
            ////接收成功,更新数据
            //md.AnalyzeResponse(request, response);
        }
    }


    public void ComTask(string com)
    {

    }

    [RelayCommand]
    [property: JsonIgnore]
    private async Task Stop()
    {
        Status = false;
    }


    /// <summary>
    /// 串口列表
    /// </summary>
    public List<ComPort> ComPorts { get; set; } = [];

    /// <summary>
    /// 用于管理串口线程
    /// </summary>
    public Dictionary<string, Task> ComTaskDict { get; set; } = [];




    /// <summary>
    /// 获取串口完整名字（包括驱动名字）
    /// </summary>
    [RelayCommand]
    [property: JsonIgnore]
    public void GetComPorts()
    {
        //清空列表
        ComPorts.Clear();
        //查找Com口
        using System.Management.ManagementObjectSearcher searcher = new("select * from Win32_PnPEntity where Name like '%(COM[0-999]%'");
        var hardInfos = searcher.Get();
        //获取串口设备列表
        foreach (var hardInfo in hardInfos)
        {
            if (hardInfo.Properties["Name"].Value != null)
            {
                string deviceName = hardInfo.Properties["Name"].Value.ToString()!;         //获取名称
                List<string> portList = [];
                //从名称中截取串口编号
                foreach (Match mch in Regex.Matches(deviceName, @"COM\d{1,3}").Cast<Match>())
                {
                    string x = mch.Value.Trim();
                    portList.Add(x);
                }
                int startIndex = deviceName.IndexOf("(");
                string port = portList[0];
                string name = deviceName[..(startIndex - 1)];
                ComPorts.Add(new ComPort(port, name));
            }
        }
    }

}

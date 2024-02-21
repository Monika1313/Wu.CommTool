namespace Wu.CommTool.Modules.MqttClient.Models;

public class ConfigFile : BindableBase
{
    public ConfigFile(FileInfo file)
    {
        File = file;
    }

    private FileInfo File { get; set; }

    /// <summary>
    /// 文件名称
    /// </summary>
    public string Name => File.Name.Replace(File.Extension, "");

    /// <summary>
    /// 文件全称
    /// </summary>
    public string FullName => File.FullName;


}

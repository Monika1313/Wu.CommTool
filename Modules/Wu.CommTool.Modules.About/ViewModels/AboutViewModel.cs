namespace Wu.CommTool.Modules.About.ViewModels;

public partial class AboutViewModel : ObservableObject
{
    public AboutViewModel()
    {

    }

    /// <summary>
    /// 打开Github链接 该方法不会被杀毒软件拦截
    /// </summary>
    private void ShowGitHub()
    {
        try
        {
            if (@"https://github.com/Monika1313/Wu.CommTool" is string link)
            {
                link = link.Replace("&", "^&");
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("cmd", $"/c start {link}")
                {
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
            }

            //System.Diagnostics.Process.Start("explorer.exe", @"https://github.com/Monika1313/Wu.CommTool");//使用资源管理器打开会被拦截
        }
        catch (Exception)
        {

        }
    }
}

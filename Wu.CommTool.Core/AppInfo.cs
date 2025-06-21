namespace Wu.CommTool.Core;

public static class AppInfo
{
    public const string Version = "1.5.2";

#if DEBUG
    public const bool Debug = true;
#else
    public const bool Debug = false;
#endif

}

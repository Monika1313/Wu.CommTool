using System.Diagnostics.Eventing.Reader;

namespace Wu.CommTool.Core;

public static class AppInfo
{
    public const string Version = "1.4.0.22";

#if DEBUG
    public const bool Debug = true;
#else
    public const bool Debug = false;
#endif

}

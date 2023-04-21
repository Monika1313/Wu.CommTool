namespace Wu.CommTool.Modules.ConvertTools.ViewModels.DesignViewModels
{
    public class TimestampConvertDesignViewModel : TimestampConvertViewModel
    {
        private static TimestampConvertDesignViewModel _Instance = new();
        public static TimestampConvertDesignViewModel Instance => _Instance ??= new();
        public TimestampConvertDesignViewModel()
        {
        }
    }
}

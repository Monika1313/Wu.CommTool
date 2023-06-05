namespace Wu.CommTool.Modules.ConvertTools.ViewModels.DesignViewModels
{
    public class ValueConvertDesignViewModel : ValueConvertViewModel
    {
        private static ValueConvertDesignViewModel _Instance = new();
        public static ValueConvertDesignViewModel Instance => _Instance ??= new();
        public ValueConvertDesignViewModel()
        {
            
        }
    }
}

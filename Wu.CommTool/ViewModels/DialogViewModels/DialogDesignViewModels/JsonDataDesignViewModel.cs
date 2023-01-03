namespace Wu.CommTool.ViewModels.DialogViewModels.DialogDesignViewModels
{
    public class JsonDataDesignViewModel : JsonDataViewModel
    {
        private static JsonDataDesignViewModel _Instance = new();
        public static JsonDataDesignViewModel Instance => _Instance ??= new();
        public JsonDataDesignViewModel()
        {

        }
    }
}

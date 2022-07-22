namespace Wu.CommTool.ViewModels.DesignViewModels
{
    public class ComToolDesignViewModel : ComToolViewModel
    {
        private static ComToolDesignViewModel _Instance;
        public static ComToolDesignViewModel Instance => _Instance ??= new();
        public ComToolDesignViewModel()
        {
            IsDrawersOpen.IsLeftDrawerOpen = true;
        }
    }
}

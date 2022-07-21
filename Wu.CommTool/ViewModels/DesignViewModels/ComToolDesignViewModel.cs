namespace Wu.Comm.ViewModels.DesignViewModels
{
    public class ComToolDesignViewModel : ComToolViewModel
    {
        private static ComToolDesignViewModel _Instance;
        public static ComToolDesignViewModel Instance => _Instance ??= new ComToolDesignViewModel();
        public ComToolDesignViewModel()
        {
            IsDrawersOpen.IsLeftDrawerOpen = true;
        }
    }
}

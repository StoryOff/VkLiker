using MahApps.Metro.Controls.Dialogs;
using VkLikerMVVM.ViewModels;

namespace VkLikerMVVM
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel(DialogCoordinator.Instance);
        }
    }
}

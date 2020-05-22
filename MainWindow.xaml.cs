using MahApps.Metro.Controls;
using System.Windows;
using System.Windows.Controls;
using VkLikerMVVM.ViewModels;

namespace VkLikerMVVM
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainVm();
        }

        //Можно переделать в SecureString для методов, которые могут ее принять
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (LoginStackPanel.DataContext != null)
            { ((dynamic)LoginStackPanel.DataContext).PasswordBox = ((PasswordBox)sender).Password; }
        }
    }
}

using deteckt.ViewModel;
using System.Windows;

namespace deteckt.View
{
    /// <summary>
    /// Логика взаимодействия для ImageShow.xaml
    /// </summary>
    public partial class ImageShow : Window
    {
        public ImageShow()
        {
            InitializeComponent();
            DataContext = new ImageShowViewModel();
        }
    }
}

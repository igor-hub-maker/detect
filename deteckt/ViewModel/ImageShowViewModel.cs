using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace deteckt.ViewModel
{
    class ImageShowViewModel : ObservableObject
    {
        private static BitmapImage _image;
        public static BitmapImage Image 
        { 
            get => _image;
            set { _image = value; }
        }
    }
}

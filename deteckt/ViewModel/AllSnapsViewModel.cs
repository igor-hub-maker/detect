using deteckt.Model;
using deteckt.View;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace deteckt.ViewModel
{
    class AllSnapsViewModel : ObservableObject
    {
        public AllSnapsViewModel()
        {
            getAllphotos();
            setImage();
            MainWindowCommand = new RelayCommand(GoMainWindow);
            NextCommand = new RelayCommand(Next);
            BackCommand = new RelayCommand(Back);
        }



        private BitmapImage _image;
        int imageIndex = 0;
        List<BitmapImage> images = new List<BitmapImage>();
        public ICommand MainWindowCommand { get; set; }
        public ICommand NextCommand { get; set; }
        public ICommand BackCommand { get; set; }
        public BitmapImage ShownImage
        {
            get { return _image; }
            set { Set(ref _image, value); }
        }
        private void GoMainWindow()
        {
            MainWindow mainWindow = new MainWindow(); 
            OpenWindow openWindow = new OpenWindow();
            openWindow.DisplayWindow(mainWindow);
        }
        private void Back()
        {
            if(imageIndex != 0)
            {
                imageIndex--;
                setImage();
            }
        }

        private void Next()
        {
            if (imageIndex != images.Count()-1)
            {
                imageIndex++;
                setImage();
            }
        }
        private void getAllphotos()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "//data//photos";
            var files = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "//data//photos");
            foreach(var file in files)
            {
                BitmapImage image = new BitmapImage(new Uri(file));
                images.Add(image);
            }   
        }
        private void setImage()
        {
            ShownImage = images[imageIndex];
        }
    }
}

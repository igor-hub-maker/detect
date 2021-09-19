using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Accord.Video.FFMPEG;
using AForge.Video;
using AForge.Video.DirectShow;
using deteckt.Extensions;
using deteckt.View;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace deteckt.ViewModel
{

    class MainWindowViewModel : ObservableObject, IDisposable
    {

        public MainWindowViewModel()
        {
            getAllphotos();
            //setImage();
            VideoDevices = new ObservableCollection<FilterInfo>();
            GetVideoDevices();
            NextCommand = new RelayCommand(NextButton);
            BackCommand = new RelayCommand(BackButton);
            StartSourceCommand = new RelayCommand(StartCamera);
            StopSourceCommand = new RelayCommand(StopCamera);
            SaveSnapshotCommand = new RelayCommand(SaveSnapshot);
            ShowImage1 = new RelayCommand(showImage1);
            ShowImage2 = new RelayCommand(showImage2);
            ShowImage3 = new RelayCommand(showImage3);
        }

      

        public ObservableCollection<FilterInfo> VideoDevices { get; set; }
        public Window window = Application.Current.MainWindow;
        int imageToViewIndex1 = 0;
        int imageToViewIndex2 = 1;
        int imageToViewIndex3 = 2;
        List<BitmapImage> images = new List<BitmapImage>();
        #region private variable
        private IVideoSource _videoSource;
        private BitmapImage _image;
        private VideoFileWriter _writer;
        private bool _recording;
        private DateTime? _firstFrameTime;
        private FilterInfo _currentDevice;
        private BitmapImage _imageToView1;
        private BitmapImage _imageToView2;
        private BitmapImage _imageToView3;
        #endregion
        #region commands
        public ICommand StartSourceCommand { get; private set; }
        public ICommand StopSourceCommand { get; private set; }
        public ICommand SaveSnapshotCommand { get; private set; }
        public ICommand OpenAllSnapsCommand { get; private set; }
        public ICommand NextCommand { get; private set; }
        public ICommand BackCommand { get; private set; }
        public ICommand ShowImage1 { get; private set; }
        public ICommand ShowImage2 { get; private set; }
        public ICommand ShowImage3 { get; private set; }
        #endregion
        #region properties
        public FilterInfo CurrentDevice
        {
            get => _currentDevice;
            set => Set(ref _currentDevice, value); 
        }
        public BitmapImage Image
        {
            get => _image;
            set => Set(ref _image, value); 
        }
        public BitmapImage ImageToView1
        {
            get => _imageToView1; 
            set => Set(ref _imageToView1, value);
        }
        public BitmapImage ImageToView2
        {
            get => _imageToView2; 
            set => Set(ref _imageToView2, value);
        }
        public BitmapImage ImageToView3
        {
            get => _imageToView3; 
            set => Set(ref _imageToView3, value);
        }
        #endregion

        private void GetVideoDevices()
        {
            var devices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo device in devices)
            {
                VideoDevices.Add(device);
            }
            if (VideoDevices.Any())
            {
                CurrentDevice = VideoDevices[0];
            }
            else
            {
                MessageBox.Show("No webcam found");
            }
        }

        private void StartCamera()
        {
            if (CurrentDevice != null)
            {
                _videoSource = new VideoCaptureDevice(CurrentDevice.MonikerString);
                _videoSource.NewFrame += NewFrameOnVideo;
                _videoSource.Start();
            }
            else
            {
                MessageBox.Show("Current device can't be null");
            }
        }

        private void NewFrameOnVideo(object _, NewFrameEventArgs eventArgs)
        {
            try
            {
                if (_recording)
                {
                    using (var bitmap = (Bitmap)eventArgs.Frame.Clone())
                    {
                        if (_firstFrameTime != null)
                        {
                            _writer.WriteVideoFrame(bitmap, DateTime.Now - _firstFrameTime.Value);
                        }
                        else
                        {
                            _writer.WriteVideoFrame(bitmap);
                            _firstFrameTime = DateTime.Now;
                        }
                    }
                }
                using (var bitmap = (Bitmap)eventArgs.Frame.Clone())
                {
                    var streamImage = bitmap.ToBitmapImage();
                    streamImage.Freeze();
                    Dispatcher.CurrentDispatcher.Invoke(() => Image = streamImage);
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show("Error on _videoSource_NewFrame:\n" + exc.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StopCamera();
            }
        }

        private void StopCamera()
        {
            if (_videoSource != null && _videoSource.IsRunning)
            {
                _videoSource.SignalToStop();
                _videoSource.NewFrame -= NewFrameOnVideo;
            }
            Image = null;
        }

        private void SaveSnapshot()
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "//Deteck//data//photos";
            var d = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            var files = Directory.GetFiles(path);
            var fileName = "//snap" + (files.Length + 1).ToString() + ".png";
            var filePath = path + fileName;
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(Image));
            using (var filestream = new FileStream(filePath, FileMode.Create))
            {
                encoder.Save(filestream);
            }
            getAllphotos();
        }

        private void NextButton()
        {
            if (imageToViewIndex3 != images.Count() - 1)
            {
                imageToViewIndex1 += 3;
                imageToViewIndex2 += 3;
                imageToViewIndex3 += 3;
                setImage();
            }
        }

        private void BackButton()
        {
            if (imageToViewIndex1 != 0)
            {
                imageToViewIndex1 -= 3;
                imageToViewIndex2 -= 3;
                imageToViewIndex3 -= 3;
                setImage();
            }
        }

        private void getAllphotos()
        {
            images.Clear();
            if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "//Deteck//data//photos"))
            {
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "//Deteck//data//photos");
            }
            var files = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "//Deteck//data//photos");
            foreach (var file in files)
            {
                BitmapImage image = new BitmapImage(new Uri(file));
                images.Add(image);
            }
            if (images.Count % 3 != 0)
            {
                int h = (images.Count % 3)+1;
                for (int i = 0; i < h; i++)
                {
                    images.Add(null);
                }
            }
            else if(images.Count == 0)
            {
                int h = 3;
                for (int i = 0; i < h; i++)
                {
                    images.Add(null);
                }
            }
            setImage();
        }
        private void setImage()
        {
                ImageToView1 = images[imageToViewIndex1];
                ImageToView2 = images[imageToViewIndex2];
                ImageToView3 = images[imageToViewIndex3];   
        }
        private void showImage1()
        {
            if (ImageToView1 != null) 
            { 
            ImageShowViewModel.Image = ImageToView1;
            ImageShow im = new ImageShow();
            im.Show();
            }
        }
        private void showImage2()
        {
            if (ImageToView2 != null)
            {
                ImageShowViewModel.Image = ImageToView2;
                ImageShow im = new ImageShow();
                im.Show();
            }
        }
        private void showImage3()
        {
            if (ImageToView3 != null)
            {
                ImageShowViewModel.Image = ImageToView3;
                ImageShow im = new ImageShow();
                im.Show();
            }
        }
        public void Dispose()
        {
            if (_videoSource != null && _videoSource.IsRunning)
            {
                _videoSource.SignalToStop();
            }
            _writer?.Dispose();
        }

    }
}


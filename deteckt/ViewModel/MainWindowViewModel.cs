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
using deteckt.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Win32;

namespace deteckt.ViewModel
{

        class MainWindowViewModel : ObservableObject, IDisposable
        {
            public MainWindowViewModel()
            {
                VideoDevices = new ObservableCollection<FilterInfo>();
                GetVideoDevices();
                StartSourceCommand = new RelayCommand(StartCamera);
                StopSourceCommand = new RelayCommand(StopCamera);
                SaveSnapshotCommand = new RelayCommand(SaveSnapshot);
            }
        public ObservableCollection<FilterInfo> VideoDevices { get; set; }
        #region private variable
        private IVideoSource _videoSource;
            private BitmapImage _image;
            private VideoFileWriter _writer;
            private bool _recording;
            private DateTime? _firstFrameTime;
            private FilterInfo _currentDevice;
        #endregion
        #region commands
        public ICommand StartSourceCommand { get; private set; }
            public ICommand StopSourceCommand { get; private set; }
            public ICommand SaveSnapshotCommand { get; private set; }
        #endregion
        #region properties
        public FilterInfo CurrentDevice
            {
                get { return _currentDevice; }
                set { Set(ref _currentDevice, value); }
            }
            public BitmapImage Image
            {
                get { return _image; }
                set { Set(ref _image, value); }
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
                _videoSource.NewFrame += video_NewFrame;
                _videoSource.Start();
            }
            else
            {
                MessageBox.Show("Current device can't be null");
            }
        }
        private void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
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
                    var bi = bitmap.ToBitmapImage();
                    bi.Freeze();
                    Dispatcher.CurrentDispatcher.Invoke(() => Image = bi);
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show("Error on _videoSource_NewFrame:\n" + exc.Message, "Error", MessageBoxButton.OK,MessageBoxImage.Error);
                StopCamera();
            }
        }
        private void StopCamera()
        {
            if (_videoSource != null && _videoSource.IsRunning)
            {
                _videoSource.SignalToStop();
                _videoSource.NewFrame -= video_NewFrame;
            }
            Image = null;
        }
        private void SaveSnapshot()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "//data//photos";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string fileName = null;
            var files = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "//data//photos//");
            fileName = "//snap" + (files.Length + 1).ToString() + ".png";
            string filePath = path + fileName;
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(Image));
            using (var filestream = new FileStream(filePath, FileMode.Create))
            {
                encoder.Save(filestream);
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


using AForge.Video;
using AForge.Video.DirectShow;
using Core;
using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;

namespace Chronos.Display
{
    public class AForgeCameraCapture : DisplayCapture
    {
        public ObservableCollection<FilterInfo> Cameras { get; set; }

        private int _index = -1;
        protected int CurrentCameraIndex
        {
            get => _index;
            set => Set(ref _index, value, nameof(CurrentCamera));
        }

        public FilterInfo CurrentCamera
        {
            get { return Cameras[CurrentCameraIndex]; }
        }

        private IVideoSource _videoSource;

        public AForgeCameraCapture()
        {
            Cameras = new ObservableCollection<FilterInfo>();

            var devices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo device in devices)
            {
                Cameras.Add(device);
            }
            if (Cameras.Any())
            {
                CurrentCameraIndex = 0;
            }
        }

        public override void Start()
        {
            _videoSource = new VideoCaptureDevice(CurrentCamera.MonikerString);
            _videoSource.NewFrame += NewFrameCallback;
            _videoSource.Start();
        }

        public override void Stop()
        {
            if (_videoSource != null && _videoSource.IsRunning)
            {
                _videoSource.Stop();
                _videoSource.NewFrame -= NewFrameCallback;
                _videoSource = null;
            }

            //Display = null;
        }

        private async void NewFrameCallback(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                Display = (Bitmap)eventArgs.Frame.Clone();
            }
            catch (Exception e)
            {
                "Camera new frame error".Print();
                Stop();
            }
            await Run();
        }

        ~AForgeCameraCapture()
        {
            Stop();
        }
    }
}

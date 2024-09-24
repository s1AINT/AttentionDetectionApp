using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Drawing;
using System.IO;

namespace AttentionDetectionApp.Utils
{
    public class CameraCapture
    {
        private VideoCaptureDevice _videoSource;
        public int FrameRate { get; private set; } 

        public CameraCapture(string cameraMonikerString)
        {
            _videoSource = new VideoCaptureDevice(cameraMonikerString);

            FrameRate = _videoSource.VideoCapabilities[0].AverageFrameRate;
            _videoSource.NewFrame += new NewFrameEventHandler(Video_NewFrame);
        }

        private Bitmap _currentFrame;

        public void StartCapture()
        {
            _videoSource.Start();
        }

        public void StopCapture()
        {
            if (_videoSource.IsRunning)
                _videoSource.SignalToStop();
        }

        private void Video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            _currentFrame = (Bitmap)eventArgs.Frame.Clone();
        }

        public byte[] CaptureFrame()
        {
            if (_currentFrame != null)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    _currentFrame.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                    return ms.ToArray(); 
                }
            }

            return null;
        }

        public static List<string> GetAvailableCameras()
        {
            var videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            return videoDevices.Cast<FilterInfo>().Select(d => d.MonikerString).ToList();
        }
    }
}

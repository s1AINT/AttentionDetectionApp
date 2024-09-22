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
        public int FrameRate { get; private set; }  // Частота кадрів камери

        public CameraCapture()
        {
            // Ініціалізація джерела відео (камера)
            var videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (videoDevices.Count == 0)
                throw new Exception("Камера не знайдена");

            _videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);

            // Визначаємо частоту кадрів
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
            // Копіюємо поточний кадр на кожному новому фреймі
            _currentFrame = (Bitmap)eventArgs.Frame.Clone();
        }

        public byte[] CaptureFrame()
        {
            if (_currentFrame != null)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    _currentFrame.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                    return ms.ToArray(); // Повертає кадр у вигляді масиву байтів
                }
            }

            return null;
        }
    }
}

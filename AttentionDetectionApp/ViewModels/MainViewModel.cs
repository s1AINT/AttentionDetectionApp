using AttentionDetectionApp.Models;
using AttentionDetectionApp.Services;
using AttentionDetectionApp.Services.Interfaces;
using AttentionDetectionApp.Utils;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace AttentionDetectionApp.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly IFrameProcessingService _frameProcessingService;
        private readonly IAttentionAnalysisService _attentionAnalysisService;
        private ObservableCollection<FaceDetectionResult> _faceDetectionResults;
        private ObservableCollection<FaceDetectionResult> _currentSubBlockFrames;
        private ObservableCollection<SubBlock> _subBlocks; 
        private Block _currentBlock; 
        private BitmapSource _currentFrame;
        private CameraCapture _cameraCapture;
        private DispatcherTimer _timer;
        private int frameRate;
        private int blockSize;

        public event PropertyChangedEventHandler PropertyChanged;

        public MainViewModel(IFrameProcessingService frameProcessingService = null, IAttentionAnalysisService attentionAnalysisService = null)
        {
            blockSize = 5;
            _cameraCapture = new CameraCapture();
            _cameraCapture.StartCapture();

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(100);
            _timer.Tick += async (s, e) => await CaptureFrameAsync();

            frameRate = CalculateFrameRate(_timer.Interval);

            _frameProcessingService = frameProcessingService ?? new FrameProcessingService(new FaceDetectionService(), frameRate);
            _attentionAnalysisService = attentionAnalysisService ?? new AttentionAnalysisService();

            FaceDetectionResults = new ObservableCollection<FaceDetectionResult>();
            _currentSubBlockFrames = new ObservableCollection<FaceDetectionResult>();
            SubBlocks = new ObservableCollection<SubBlock>(); 

            _frameProcessingService.FrameProcessed += OnFrameProcessed;
            _timer.Start();
        }

        private int CalculateFrameRate(TimeSpan interval)
        {
            return (int)(1000 / interval.TotalMilliseconds);
        }

        public ICommand CaptureFrameCommand => new RelayCommand(async () => await CaptureFrameAsync());

        private async Task CaptureFrameAsync()
        {
            byte[] frameData = _cameraCapture.CaptureFrame();
            _frameProcessingService.ProcessFrame(frameData);
        }

        private void OnFrameProcessed(byte[] frameData, FaceDetectionResult result)
        {
            var bitmapImage = ConvertByteArrayToBitmapImage(frameData);

            var annotatedFrame = DrawFacialLandmarks(bitmapImage, result);

            CurrentFrame = annotatedFrame;

            _currentSubBlockFrames.Add(result);

            if (_currentSubBlockFrames.Count >= frameRate)
            {
                var subBlock = new SubBlock(
                    _currentSubBlockFrames.Select(f => _frameProcessingService.DetermineFrameSubStatus(f)).ToList(),
                    _attentionAnalysisService.AnalyzeSubBlock(_currentSubBlockFrames.Select(f => _frameProcessingService.DetermineFrameSubStatus(f)).ToList())
                );

                SubBlocks.Add(subBlock);

                _currentSubBlockFrames.Clear();
            }

            if (SubBlocks.Count >= blockSize)
            {
                CurrentBlock = new Block(
                    SubBlocks.ToList(),
                    _attentionAnalysisService.AnalyzeBlock(SubBlocks.ToList())
                );

                SubBlocks.Clear();
            }
        }


        private WriteableBitmap DrawFacialLandmarks(BitmapImage image, FaceDetectionResult result)
        {
            var writeableBitmap = new WriteableBitmap(image);

            if (result.IsFaceDetected && result.LandmarkPoints != null)
            {
                using (var context = writeableBitmap.GetBitmapContext())
                {
                    foreach (var point in result.LandmarkPoints.Values)
                    {
                        DrawPoint(writeableBitmap, point.X, point.Y, Colors.Red, 3);
                    }
                }
            }

            return writeableBitmap;
        }

        private void DrawPoint(WriteableBitmap bitmap, int x, int y, Color color, int radius)
        {
            int startX = Math.Max(0, x - radius);
            int endX = Math.Min(bitmap.PixelWidth - 1, x + radius);
            int startY = Math.Max(0, y - radius);
            int endY = Math.Min(bitmap.PixelHeight - 1, y + radius);

            for (int i = startX; i <= endX; i++)
            {
                for (int j = startY; j <= endY; j++)
                {
                    double distance = Math.Sqrt((i - x) * (i - x) + (j - y) * (j - y));
                    if (distance <= radius)
                    {
                        bitmap.SetPixel(i, j, color);
                    }
                }
            }
        }

        public ObservableCollection<FaceDetectionResult> FaceDetectionResults
        {
            get => _faceDetectionResults;
            set
            {
                _faceDetectionResults = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<SubBlock> SubBlocks 
        {
            get => _subBlocks;
            set
            {
                _subBlocks = value;
                OnPropertyChanged();
            }
        }

        public Block CurrentBlock
        {
            get => _currentBlock;
            set
            {
                _currentBlock = value;
                OnPropertyChanged();
            }
        }

        public BitmapSource CurrentFrame
        {
            get => _currentFrame;
            set
            {
                _currentFrame = value;
                OnPropertyChanged();
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private BitmapImage ConvertByteArrayToBitmapImage(byte[] byteArray)
        {
            using (MemoryStream ms = new MemoryStream(byteArray))
            {
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = ms;
                image.EndInit();
                return image;
            }
        }
    }
}

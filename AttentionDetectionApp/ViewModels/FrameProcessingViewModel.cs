using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using AttentionDetectionApp.Models;
using AttentionDetectionApp.Services.Interfaces;
using AttentionDetectionApp.Services;
using System.IO;

namespace AttentionDetectionApp.ViewModels
{
    public class FrameProcessingViewModel : BaseViewModel
    {
        private readonly IFrameProcessingService _frameProcessingService;
        private readonly IAttentionAnalysisService _attentionAnalysisService;
        private ObservableCollection<FaceDetectionResult> _faceDetectionResults;
        private ObservableCollection<FaceDetectionResult> _currentSubBlockFrames;
        private ObservableCollection<SubBlock> _subBlocks;
        private Block _currentBlock;
        private BitmapSource _currentFrame;
        private int frameRate;
        private int blockSize;

        public FrameProcessingViewModel(IFrameProcessingService frameProcessingService, IAttentionAnalysisService attentionAnalysisService, int frameRate, int blockSize)
        {
            this.frameRate = frameRate;
            this.blockSize = blockSize;

            _frameProcessingService = frameProcessingService ?? new FrameProcessingService(new FaceDetectionService(), frameRate);
            _attentionAnalysisService = attentionAnalysisService ?? new AttentionAnalysisService();

            FaceDetectionResults = new ObservableCollection<FaceDetectionResult>();
            _currentSubBlockFrames = new ObservableCollection<FaceDetectionResult>();
            SubBlocks = new ObservableCollection<SubBlock>();

            _frameProcessingService.FrameProcessed += OnFrameProcessed;
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

        public async Task CaptureFrameAsync(byte[] frameData)
        {
            if (frameData != null)
            {
                _frameProcessingService.ProcessFrame(frameData);
            }
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
                CurrentBlock = new Block(SubBlocks.ToList(), _attentionAnalysisService.AnalyzeBlock(SubBlocks.ToList()));
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

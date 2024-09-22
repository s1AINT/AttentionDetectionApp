using AttentionDetectionApp.Models;
using AttentionDetectionApp.Models.Statuses;
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
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace AttentionDetectionApp.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly IFrameProcessingService _frameProcessingService;
        private readonly IAttentionAnalysisService _attentionAnalysisService;
        private ObservableCollection<FaceDetectionResult> _faceDetectionResults;
        private ObservableCollection<SubBlockStatus> _subBlockStatuses;
        private BlockStatus _currentBlockStatus;
        private BitmapSource _currentFrame;
        private CameraCapture _cameraCapture;
        private DispatcherTimer _timer;

        public event PropertyChangedEventHandler PropertyChanged;

        public MainViewModel(IFrameProcessingService frameProcessingService = null, IAttentionAnalysisService attentionAnalysisService = null)
        {
            // Ініціалізуємо об'єкт для захоплення кадрів з камери
            _cameraCapture = new CameraCapture();
            _cameraCapture.StartCapture();  // Запускаємо захоплення кадрів

            // Створюємо таймер для захоплення кадрів
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(100);  // Оновлюємо кадри кожні 100 мс
            _timer.Tick += async (s, e) => await CaptureFrameAsync();

            // Обчислюємо частоту кадрів (fps) на основі інтервалу таймера
            int frameRate = CalculateFrameRate(_timer.Interval);

            if (frameProcessingService == null)
                _frameProcessingService = new FrameProcessingService(new FaceDetectionService(), frameRate);
            else
                _frameProcessingService = frameProcessingService;

            if (attentionAnalysisService == null)
                _attentionAnalysisService = new AttentionAnalysisService();
            else
                _attentionAnalysisService = attentionAnalysisService;

            FaceDetectionResults = new ObservableCollection<FaceDetectionResult>();
            SubBlockStatuses = new ObservableCollection<SubBlockStatus>();

            // Підписуємося на подію обробки кадру
            _frameProcessingService.FrameProcessed += OnFrameProcessed;

            _timer.Start();
        }

        // Метод для обчислення кількості кадрів в секунду (fps)
        private int CalculateFrameRate(TimeSpan interval)
        {
            // fps = 1000 / інтервал в мілісекундах
            return (int)(1000 / interval.TotalMilliseconds);
        }

        // Команда для захоплення кадру вручну
        public ICommand CaptureFrameCommand => new RelayCommand(async () => await CaptureFrameAsync());

        private async Task CaptureFrameAsync()
        {
            // Захоплюємо кадр
            byte[] frameData = _cameraCapture.CaptureFrame();
            _frameProcessingService.ProcessFrame(frameData);
        }

        private void OnFrameProcessed(byte[] frameData, FaceDetectionResult result)
        {
            // Оновлюємо поточний кадр
            CurrentFrame = ConvertByteArrayToBitmapImage(frameData);

            // Додаємо результат в колекцію
            FaceDetectionResults.Add(result);

            // Аналізуємо статус підблоку
            var subBlockStatus = _attentionAnalysisService.AnalyzeSubBlock(FaceDetectionResults.Select(f => _frameProcessingService.DetermineFrameSubStatus(f)).ToList());
            SubBlockStatuses.Add(subBlockStatus);

            if (SubBlockStatuses.Count >= 5)  // Припустимо, що 5 підблоків достатньо для аналізу блоку
            {
                // Аналізуємо статус блоку
                _currentBlockStatus = _attentionAnalysisService.AnalyzeBlock(SubBlockStatuses.ToList());
                SubBlockStatuses.Clear();
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

        public ObservableCollection<SubBlockStatus> SubBlockStatuses
        {
            get => _subBlockStatuses;
            set
            {
                _subBlockStatuses = value;
                OnPropertyChanged();
            }
        }

        public BlockStatus CurrentBlockStatus
        {
            get => _currentBlockStatus;
            set
            {
                _currentBlockStatus = value;
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

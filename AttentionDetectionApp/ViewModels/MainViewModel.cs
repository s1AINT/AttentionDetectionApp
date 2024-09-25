using System.Windows.Input;
using System.Windows.Threading;
using AttentionDetectionApp.Services.Interfaces;
using AttentionDetectionApp.Utils;

namespace AttentionDetectionApp.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private CameraSelectionViewModel _cameraSelectionViewModel;
        private FrameProcessingViewModel _frameProcessingViewModel;
        private bool _isCapturing;
        private DispatcherTimer _timer;
        int _timerInterval;

        public MainViewModel(IFrameProcessingService frameProcessingService = null, IAttentionAnalysisService attentionAnalysisService = null)
        {
            _timerInterval = 200;
            _cameraSelectionViewModel = new CameraSelectionViewModel();
            _frameProcessingViewModel = new FrameProcessingViewModel(frameProcessingService, attentionAnalysisService, CalculateFrameRate(TimeSpan.FromMilliseconds(_timerInterval)), 5);

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(_timerInterval);
            _timer.Tick += async (s, e) => await CaptureFrameAsync();

            IsCapturing = false;
        }

        public CameraSelectionViewModel CameraSelectionViewModel
        {
            get => _cameraSelectionViewModel;
            set => SetProperty(ref _cameraSelectionViewModel, value);
        }

        public FrameProcessingViewModel FrameProcessingViewModel
        {
            get => _frameProcessingViewModel;
            set => SetProperty(ref _frameProcessingViewModel, value);
        }

        public bool IsCapturing
        {
            get => _isCapturing;
            set => SetProperty(ref _isCapturing, value);
        }

        public ICommand StartCaptureCommand => new RelayCommand(StartCapture);
        public ICommand StopCaptureCommand => new RelayCommand(StopCapture);

        private void StartCapture()
        {
            CameraSelectionViewModel.StartCamera();
            _timer.Start();
            IsCapturing = true;
        }

        private void StopCapture()
        {
            CameraSelectionViewModel.StopCamera();
            _timer.Stop();
            IsCapturing = false;
        }

        private async Task CaptureFrameAsync()
        {
            if (!IsCapturing) return;

            byte[] frameData = CameraSelectionViewModel.CaptureFrame();
            if (frameData != null)
            {
                await FrameProcessingViewModel.CaptureFrameAsync(frameData);
            }
        }

        private int CalculateFrameRate(TimeSpan interval)
        {
            return (int)(1000 / interval.TotalMilliseconds);
        }
    }
}

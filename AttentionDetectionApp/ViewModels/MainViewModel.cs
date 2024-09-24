using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
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

        public MainViewModel(IFrameProcessingService frameProcessingService = null, IAttentionAnalysisService attentionAnalysisService = null)
        {
            _cameraSelectionViewModel = new CameraSelectionViewModel();
            _frameProcessingViewModel = new FrameProcessingViewModel(frameProcessingService, attentionAnalysisService, CalculateFrameRate(TimeSpan.FromMilliseconds(50)), 5);

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(50);
            _timer.Tick += async (s, e) => await CaptureFrameAsync();

            IsCapturing = false;
        }

        public CameraSelectionViewModel CameraSelectionViewModel
        {
            get => _cameraSelectionViewModel;
            set
            {
                _cameraSelectionViewModel = value;
                OnPropertyChanged();
            }
        }

        public FrameProcessingViewModel FrameProcessingViewModel
        {
            get => _frameProcessingViewModel;
            set
            {
                _frameProcessingViewModel = value;
                OnPropertyChanged();
            }
        }

        public bool IsCapturing
        {
            get => _isCapturing;
            set
            {
                _isCapturing = value;
                OnPropertyChanged();
            }
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

using AForge.Video.DirectShow;
using AttentionDetectionApp.Utils;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace AttentionDetectionApp.ViewModels
{
    public class CameraSelectionViewModel : INotifyPropertyChanged
    {
        private CameraCapture _cameraCapture;
        private string _selectedCamera;
        private ObservableCollection<string> _availableCameras;

        public event PropertyChangedEventHandler PropertyChanged;

        public CameraSelectionViewModel()
        {
            LoadAvailableCameras();
        }

        public ObservableCollection<string> AvailableCameras
        {
            get => _availableCameras;
            set
            {
                _availableCameras = value;
                OnPropertyChanged();
            }
        }

        public string SelectedCamera
        {
            get => _selectedCamera;
            set
            {
                _selectedCamera = value;
                OnPropertyChanged();
            }
        }

        public void StartCamera()
        {
            if (string.IsNullOrEmpty(SelectedCamera))
            {
                MessageBox.Show("Будь ласка, виберіть камеру перед стартом.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            var selectedDevice = videoDevices.Cast<FilterInfo>().FirstOrDefault(d => d.Name == SelectedCamera);
            if (selectedDevice != null)
            {
                try
                {
                    _cameraCapture = new CameraCapture(selectedDevice.MonikerString);
                    _cameraCapture.StartCapture();
                }
                catch
                {
                    MessageBox.Show("Не вдалося запустити камеру. Будь ласка, перевірте підключення або виберіть іншу камеру.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Обрану камеру не вдалося знайти. Спробуйте оновити список камер.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void StopCamera()
        {
            if (_cameraCapture != null)
            {
                _cameraCapture.StopCapture();
                MessageBox.Show("Камера зупинена успішно.", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public byte[] CaptureFrame()
        {
            return _cameraCapture?.CaptureFrame();
        }

        private void LoadAvailableCameras()
        {
            var videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            AvailableCameras = new ObservableCollection<string>(videoDevices.Cast<FilterInfo>().Select(d => d.Name).ToList());

            if (AvailableCameras.Count == 0)
            {
                MessageBox.Show("Не знайдено жодної камери. Будь ласка, підключіть камеру і спробуйте знову.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                SelectedCamera = AvailableCameras.FirstOrDefault();
            }
        }

        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

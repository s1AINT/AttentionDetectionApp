using AttentionDetectionApp.Services;
using AttentionDetectionApp.Services.Interfaces;
using AttentionDetectionApp.ViewModels;
using System.Windows;


namespace AttentionDetectionApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new MainViewModel();
        }
    }
}
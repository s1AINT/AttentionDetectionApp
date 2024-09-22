using AttentionDetectionApp.Models;
using AttentionDetectionApp.Models.Statuses;


namespace AttentionDetectionApp.Services.Interfaces
{
    public interface IFrameProcessingService
    {
        event Action<byte[], FaceDetectionResult> FrameProcessed;  // Подія для обробленого кадру
        void ProcessFrame(byte[] frameData);  // Метод для обробки кадру
        FrameSubStatus DetermineFrameSubStatus(FaceDetectionResult result);  // Метод для визначення статусу кадру
    }
}

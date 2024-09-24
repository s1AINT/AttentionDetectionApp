using AttentionDetectionApp.Models;
using AttentionDetectionApp.Models.Statuses;


namespace AttentionDetectionApp.Services.Interfaces
{
    public interface IFrameProcessingService
    {
        event Action<byte[], FaceDetectionResult> FrameProcessed;
        void ProcessFrame(byte[] frameData); 
        FrameSubStatus DetermineFrameSubStatus(FaceDetectionResult result);
    }
}

using AttentionDetectionApp.Models;


namespace AttentionDetectionApp.Services.Interfaces
{
    public interface IFaceDetectionService
    {
        FaceDetectionResult DetectFaceAndAttributes(byte[] frameData);
    }
}

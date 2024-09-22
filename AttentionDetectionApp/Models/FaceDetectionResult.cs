


using DlibDotNet;

namespace AttentionDetectionApp.Models
{
    public class FaceDetectionResult
    {
        public bool IsFaceDetected { get; set; }
        public double LeftEyeOpenProbability { get; set; }
        public double RightEyeOpenProbability { get; set; }
        public double HeadRotationAngleYaw { get; set; }
        public double HeadRotationAnglePitch { get; set; }
        public double HeadRotationAngleRoll { get; set; }
        public Dictionary<int, Point> LandmarkPoints { get; set; } 
    }
}

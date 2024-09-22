

namespace AttentionDetectionApp.Models
{
    public class FaceDetectionResult
    {
        public bool IsFaceDetected { get; set; }
        public double LeftEyeOpenProbability { get; set; }
        public double RightEyeOpenProbability { get; set; }
        public double HeadRotationAngleYaw { get; set; }  // Кут повороту голови (вліво/вправо)
        public double HeadRotationAnglePitch { get; set; } // Кут нахилу голови (вверх/вниз)
        public double HeadRotationAngleRoll { get; set; } // Кут нахилу голови вбік (ліво/право)
    }
}

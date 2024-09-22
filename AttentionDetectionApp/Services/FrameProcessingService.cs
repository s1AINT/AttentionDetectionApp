using AttentionDetectionApp.Models;
using AttentionDetectionApp.Models.Statuses;
using AttentionDetectionApp.Services.Interfaces;

namespace AttentionDetectionApp.Services
{
    public class FrameProcessingService : IFrameProcessingService
    {
        private readonly IFaceDetectionService _faceDetectionService;
        private readonly List<FaceDetectionResult> _frameBlock;
        private readonly List<FrameSubStatus> _frameSubStatuses;
        private readonly List<SubBlockStatus> _subBlockStatuses;
        private int _framesPerSubBlock;
        private const int SubBlocksPerBlock = 5;  // 5 підблоків на блок

        private int _currentFrameCount = 0;  // Лічильник кадрів
        private int _currentSubBlockIndex = 0;  // Лічильник підблоків

        public event Action<byte[], FaceDetectionResult> FrameProcessed;  // Подія для обробленого кадру

        public FrameProcessingService(IFaceDetectionService faceDetectionService, int framesPerSecond)
        {
            _faceDetectionService = faceDetectionService;
            _framesPerSubBlock = framesPerSecond;  // Кількість кадрів у підблоці визначається частотою кадрів
            _frameBlock = new List<FaceDetectionResult>(_framesPerSubBlock);
            _frameSubStatuses = new List<FrameSubStatus>(_framesPerSubBlock);
            _subBlockStatuses = new List<SubBlockStatus>(SubBlocksPerBlock);
        }

        public void ProcessFrame(byte[] frameData)
        {
            var faceDetectionResult = _faceDetectionService.DetectFaceAndAttributes(frameData);
            var frameSubStatus = DetermineFrameSubStatus(faceDetectionResult);
            _frameSubStatuses.Add(frameSubStatus);

            FrameProcessed?.Invoke(frameData, faceDetectionResult);

            _currentFrameCount++;

            if (_frameSubStatuses.Count >= _framesPerSubBlock)
            {
                // Аналізуємо підблок
                AttentionAnalysisService analysisService = new AttentionAnalysisService();
                var subBlockStatus = analysisService.AnalyzeSubBlock(_frameSubStatuses);
                _subBlockStatuses.Add(subBlockStatus);

                _currentSubBlockIndex++;
                _frameSubStatuses.Clear();

                if (_currentSubBlockIndex >= SubBlocksPerBlock)
                {
                    // Аналізуємо блок
                    var blockStatus = analysisService.AnalyzeBlock(_subBlockStatuses);
                    _subBlockStatuses.Clear();
                    _currentSubBlockIndex = 0;
                }
            }
        }

        public FrameSubStatus DetermineFrameSubStatus(FaceDetectionResult result)
        {
            if (result.LeftEyeOpenProbability < 0.1 && result.RightEyeOpenProbability < 0.1)
            {
                return FrameSubStatus.ClosedEyes;
            }

            if (result.HeadRotationAngleYaw > 20)
            {
                return FrameSubStatus.HeadTurnedRight;
            }
            if (result.HeadRotationAngleYaw < -20)
            {
                return FrameSubStatus.HeadTurnedLeft;
            }

            return FrameSubStatus.OpenEyes;  // Голова прямо і очі відкриті
        }
    }
}

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
        private readonly List<SubBlock> _subBlocks;
        private int _framesPerSubBlock;
        private const int SubBlocksPerBlock = 5;  

        private int _currentFrameCount = 0; 
        private int _currentSubBlockIndex = 0; 

        public event Action<byte[], FaceDetectionResult> FrameProcessed;

        public FrameProcessingService(IFaceDetectionService faceDetectionService, int framesPerSecond)
        {
            _faceDetectionService = faceDetectionService;
            _framesPerSubBlock = framesPerSecond; 
            _frameBlock = new List<FaceDetectionResult>(_framesPerSubBlock);
            _frameSubStatuses = new List<FrameSubStatus>(_framesPerSubBlock);
            _subBlocks = new List<SubBlock>(SubBlocksPerBlock);
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
                AttentionAnalysisService analysisService = new AttentionAnalysisService();
                var subBlockStatus = analysisService.AnalyzeSubBlock(_frameSubStatuses);

                SubBlock subBlock = new SubBlock(new List<FrameSubStatus>(_frameSubStatuses), subBlockStatus);
                _subBlocks.Add(subBlock);

                _currentSubBlockIndex++;
                _frameSubStatuses.Clear();

                if (_currentSubBlockIndex >= SubBlocksPerBlock)
                {
                    var blockStatus = analysisService.AnalyzeBlock(_subBlocks);

                    Block block = new Block(new List<SubBlock>(_subBlocks), blockStatus);
                    _subBlocks.Clear();
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

            return FrameSubStatus.OpenEyes;
        }
    }
}

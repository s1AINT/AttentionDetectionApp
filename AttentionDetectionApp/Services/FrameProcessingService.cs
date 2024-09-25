using AttentionDetectionApp.Models;
using AttentionDetectionApp.Models.Statuses;
using AttentionDetectionApp.Services.Interfaces;
using System.Threading.Tasks;

namespace AttentionDetectionApp.Services
{
    public class FrameProcessingService : IFrameProcessingService
    {
        private readonly IFaceDetectionService _faceDetectionService;
        private readonly List<FrameStatus> _frameSubStatuses;
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
            _frameSubStatuses = new List<FrameStatus>(_framesPerSubBlock);
            _subBlocks = new List<SubBlock>(SubBlocksPerBlock);
        }

        public async Task ProcessFramesAsync(List<byte[]> frameDataList)
        {
            await Task.Run(() =>
            {
                Parallel.ForEach(frameDataList, frameData =>
                {
                    ProcessFrame(frameData);
                });
            });
        }

        public void ProcessFrame(byte[] frameData)
        {
            var faceDetectionResult = _faceDetectionService.DetectFaceAndAttributes(frameData);
            var frameSubStatus = DetermineFrameSubStatus(faceDetectionResult);
            lock (_frameSubStatuses)
            {
                _frameSubStatuses.Add(frameSubStatus);
            }

            FrameProcessed?.Invoke(frameData, faceDetectionResult);

            lock (_frameSubStatuses)
            {
                _currentFrameCount++;

                if (_frameSubStatuses.Count >= _framesPerSubBlock)
                {
                    AttentionAnalysisService analysisService = new AttentionAnalysisService();
                    var subBlockStatus = analysisService.AnalyzeSubBlock(_frameSubStatuses);

                    SubBlock subBlock = new SubBlock(new List<FrameStatus>(_frameSubStatuses), subBlockStatus);
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
        }

        public FrameStatus DetermineFrameSubStatus(FaceDetectionResult result)
        {
            if (!result.IsFaceDetected || result.LandmarkPoints == null || result.LandmarkPoints.Count == 0)
            {
                return FrameStatus.FaceNotDetected;
            }

            if (result.HeadRotationAngleYaw > 0.66)
            {
                return FrameStatus.HeadTurnedRight;
            }

            if (result.HeadRotationAngleYaw < -0.66)
            {
                return FrameStatus.HeadTurnedLeft;
            }

            if (result.LeftEyeOpenProbability < 0.1 && result.RightEyeOpenProbability < 0.1)
            {
                return FrameStatus.ClosedEyes;
            }

            return FrameStatus.OpenEyes;
        }

    }
}

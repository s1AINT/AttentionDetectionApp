using AttentionDetectionApp.Models;
using AttentionDetectionApp.Models.Statuses;
using AttentionDetectionApp.Services.Interfaces;

namespace AttentionDetectionApp.Services
{
    public class AttentionAnalysisService : IAttentionAnalysisService
    {
        private const int MinSubBlocksForBlockAnalysis = 5;

        public BlockStatus AnalyzeBlock(List<SubBlock> subBlocks)
        {
            if (subBlocks.Count < MinSubBlocksForBlockAnalysis)
            {
                return BlockStatus.LostAttention; 
            }

            List<SubBlockStatus> subBlockStatuses = new List<SubBlockStatus>();
            foreach (var subBlock in subBlocks)
            {
                subBlockStatuses.Add(subBlock.Status);
            }

            AttentionPattern pattern = new AttentionPattern();
            return pattern.DetermineBlockStatus(subBlockStatuses);
        }

        public SubBlockStatus AnalyzeSubBlock(List<FrameStatus> frameSubStatuses)
        {
            if (frameSubStatuses.TrueForAll(status => status == FrameStatus.FaceNotDetected))
            {
                return SubBlockStatus.PersonNotFound;
            }

            if (frameSubStatuses.TrueForAll(status => status == FrameStatus.ClosedEyes))
            {
                return SubBlockStatus.Sleeping;
            }

            if (frameSubStatuses.TrueForAll(status => status == FrameStatus.HeadTurnedLeft || status == FrameStatus.HeadTurnedRight))
            {
                return SubBlockStatus.HeadTurned;
            }

            if (frameSubStatuses.Exists(status => status == FrameStatus.OpenEyes) &&
                frameSubStatuses.Exists(status => status == FrameStatus.ClosedEyes))
            {
                return SubBlockStatus.Blinked;
            }

            return SubBlockStatus.OpenEyes;
        }

    }
}

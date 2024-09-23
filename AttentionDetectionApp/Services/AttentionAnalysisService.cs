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

        public SubBlockStatus AnalyzeSubBlock(List<FrameSubStatus> frameSubStatuses)
        {
            if (frameSubStatuses.TrueForAll(status => status == FrameSubStatus.ClosedEyes))
            {
                return SubBlockStatus.Sleeping;
            }

            if (frameSubStatuses.Exists(status => status == FrameSubStatus.HeadTurnedLeft || status == FrameSubStatus.HeadTurnedRight))
            {
                return SubBlockStatus.HeadTurned;
            }

            if (frameSubStatuses.Exists(status => status == FrameSubStatus.OpenEyes) &&
                frameSubStatuses.Exists(status => status == FrameSubStatus.ClosedEyes))
            {
                return SubBlockStatus.Blinked;
            }

            return SubBlockStatus.OpenEyes;
        }
    }
}

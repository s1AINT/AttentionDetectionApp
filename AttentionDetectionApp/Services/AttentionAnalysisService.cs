using AttentionDetectionApp.Models;
using AttentionDetectionApp.Models.Statuses;
using AttentionDetectionApp.Services.Interfaces;

namespace AttentionDetectionApp.Services
{
    public class AttentionAnalysisService : IAttentionAnalysisService
    {
        private const int MinSubBlocksForBlockAnalysis = 5; // 5 підблоків для аналізу блоку

        public BlockStatus AnalyzeBlock(List<SubBlockStatus> subBlockHistory)
        {
            if (subBlockHistory.Count < MinSubBlocksForBlockAnalysis)
            {
                return BlockStatus.LostAttention;  // Недостатньо підблоків для аналізу
            }

            // Аналізуємо підблоки для визначення статусу блоку
            AttentionPattern pattern = new AttentionPattern();
            return pattern.DetermineBlockStatus(subBlockHistory);
        }

        public SubBlockStatus AnalyzeSubBlock(List<FrameSubStatus> frameSubStatuses)
        {
            // Логіка визначення статусу підблоку на основі кадрів
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

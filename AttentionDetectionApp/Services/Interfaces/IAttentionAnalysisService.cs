using AttentionDetectionApp.Models;
using AttentionDetectionApp.Models.Statuses;


namespace AttentionDetectionApp.Services.Interfaces
{
    public interface IAttentionAnalysisService
    {
        public BlockStatus AnalyzeBlock(List<SubBlock> subBlocks);
        SubBlockStatus AnalyzeSubBlock(List<FrameStatus> frameSubStatuses);
    }
}

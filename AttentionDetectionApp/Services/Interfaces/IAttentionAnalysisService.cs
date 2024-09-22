using AttentionDetectionApp.Models;
using AttentionDetectionApp.Models.Statuses;


namespace AttentionDetectionApp.Services.Interfaces
{
    public interface IAttentionAnalysisService
    {
        BlockStatus AnalyzeBlock(List<SubBlockStatus> subBlockHistory);
        SubBlockStatus AnalyzeSubBlock(List<FrameSubStatus> frameSubStatuses);
    }
}

using AttentionDetectionApp.Models.Statuses;


namespace AttentionDetectionApp.Models
{
    public class SubBlock
    {
        public List<FrameSubStatus> Frames { get; set; } = new List<FrameSubStatus>();
        public SubBlockStatus Status { get; set; }

        public SubBlock(List<FrameSubStatus> frames, SubBlockStatus status)
        {
            Frames = frames;
            Status = status;
        }
    }
}

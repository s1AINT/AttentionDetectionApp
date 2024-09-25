using AttentionDetectionApp.Models.Statuses;


namespace AttentionDetectionApp.Models
{
    public class SubBlock
    {
        public List<FrameStatus> Frames { get; set; } = new List<FrameStatus>();
        public SubBlockStatus Status { get; set; }

        public SubBlock(List<FrameStatus> frames, SubBlockStatus status)
        {
            Frames = frames;
            Status = status;
        }
    }
}

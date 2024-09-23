

using AttentionDetectionApp.Models.Statuses;

namespace AttentionDetectionApp.Models
{
    public class AttentionPattern
    {
        public BlockStatus DetermineBlockStatus(List<SubBlockStatus> subBlockStatuses)
        {
            if (subBlockStatuses.TrueForAll(status => status == SubBlockStatus.Sleeping))
            {
                return BlockStatus.Sleepy;
            }

            if (subBlockStatuses.Exists(status => status == SubBlockStatus.HeadTurned))
            {
                return BlockStatus.Distracted;
            }

            if (subBlockStatuses.TrueForAll(status => status == SubBlockStatus.OpenEyes))
            {
                return BlockStatus.HighlyFocused;
            }

            int blinkCount = subBlockStatuses.FindAll(status => status == SubBlockStatus.Blinked).Count;
            if (blinkCount > subBlockStatuses.Count / 2)
            {
                return BlockStatus.ModeratelyFocused;
            }

            return BlockStatus.LostAttention;
        }
    }
}

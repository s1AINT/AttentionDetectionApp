using AttentionDetectionApp.Models.Statuses;


namespace AttentionDetectionApp.Models
{
    public class Block
    {
        public List<SubBlock> SubBlocks { get; set; } = new List<SubBlock>(); 
        public BlockStatus Status { get; set; } 

        public Block(List<SubBlock> subBlocks, BlockStatus status)
        {
            SubBlocks = subBlocks;
            Status = status;
        }
    }
}

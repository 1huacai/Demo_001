using Project;

namespace Demo
{
    public class BlockData
    {
        public int row;
        public int col;
        public BlockType type;

        public BlockData(int row, int col, BlockType type)
        {
            this.row = row;
            this.col = col;
            this.type = type;
        }
    }
}
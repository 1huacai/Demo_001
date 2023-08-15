using Project;

namespace Demo
{
    public class BlockData
    {
        public int row;
        public int col;
        public BlockShape Shape;

        public BlockData(int row, int col, BlockShape shape)
        {
            this.row = row;
            this.col = col;
            this.Shape = shape;
        }
    }
}
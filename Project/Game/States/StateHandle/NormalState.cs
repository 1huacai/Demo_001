using Project;

namespace Demo
{
    public class NormalState:Statebase
    {
        public NormalState(GameManger manger) : base(manger)
        {
            
        }

        public override void Enter(Block block)
        {
            if (block.Type == BlockType.None)
                return;
            block.State = BlockState.Normal;
        }

        public override void Update(Block block)
        {
            if (block.Type == BlockType.None)
                return;
            //下方棋子Type为None且block的row = 1
        }
        
        
        public override void OnBlockOperation(int row, int col, BlockOperation operation)
        {
            base.OnBlockOperation(row, col, operation);
            var block = _gameManger.blockMatrix[row, col - 1];
            if (operation == BlockOperation.TouchDown)
            {
                block.IsSelected = true;
            }

            if (operation == BlockOperation.TouchUp)
            {
                block.IsSelected = false;
            }
            
        }
    }
}
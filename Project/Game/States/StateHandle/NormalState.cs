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
            base.Enter(block);
            block.State = BlockState.Normal;
        }

        public override void Update(Block block)
        {
            base.Update(block);
        }

        public override void Exit(Block block)
        {
            base.Exit(block);
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
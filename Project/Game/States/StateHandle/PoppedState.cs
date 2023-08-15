using Project;

namespace Demo
{
    public class PoppedState : Statebase
    {
        public PoppedState(SelfGameController controller) : base(controller)
        {
        }

        public override void Enter(Block block)
        {
            if (block.Shape == BlockShape.None)
                return;
            block.State = BlockState.Popped;
            block.Shape = BlockShape.None;
            block.Chain = false;
            // for (int i = 0; i < _gameManger.pressureBlocks.Count; i++)
            // {
            //     var pressureBlock = _gameManger.pressureBlocks[i];
            //     pressureBlock.UnlockPressureBlock(block.Row,block.Col);
            // }
        }

        public override void Update(Block block)
        {
            //暂时
            if(block == null)
                return;
            if (block.Shape == BlockShape.None)
                return;
            //block类型变为空棋子
            StateManger._instance.ChangeState(BlockState.Normal, block);
        }
    }
}
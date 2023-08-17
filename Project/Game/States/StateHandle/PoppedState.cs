using Project;

namespace Demo
{
    public class PoppedState : Statebase
    {
        public PoppedState(Controller controller) : base(controller)
        {
        }

        public override void Enter(Block block)
        {
            if (block.Shape == BlockShape.None)
                return;
            //己方棋子
            block.State = BlockState.Popped;
            block.Shape = BlockShape.None;
            block.Chain = false;
            
            //对手棋子
            var otherBlock = OtherGameController.Inst.blockMatrix[block.Row, block.Col - 1];
            otherBlock.Shape = BlockShape.None;
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
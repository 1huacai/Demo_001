using Project;

namespace Demo
{
    public class PoppedState:Statebase
    {
        public PoppedState(GameManger manger) : base(manger)
        {
        }

        public override void Enter(Block block)
        {
            base.Enter(block);
            block.State = BlockState.Popped;
        }

        public override void Update(Block block)
        {
            base.Update(block);
            //block类型变为空棋子
            block.type = BlockType.None;
            StateManger._instance.ChangeState(BlockState.Normal,block);
        }

        public override void Exit(Block block)
        {
            base.Exit(block);
            
        }
    }
}
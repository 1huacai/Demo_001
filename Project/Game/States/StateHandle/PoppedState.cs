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
            if (block.Type == BlockType.None)
                return;
            block.State = BlockState.Popped;
        }

        public override void Update(Block block)
        {
            if (block.Type == BlockType.None)
                return;
            //block类型变为空棋子
            StateManger._instance.ChangeState(BlockState.Normal,block);
            block.Type = BlockType.None;
        }
        
    }
}
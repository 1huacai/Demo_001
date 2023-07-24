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
            //倒计时结束popedTimer
            // if (popedTimer == 0)
            // {
            //     Exit(block);
            // }
        }

        public override void Exit(Block block)
        {
            base.Exit(block);
            //block类型变为空棋子
            block.type = BlockType.None;
        }
    }
}
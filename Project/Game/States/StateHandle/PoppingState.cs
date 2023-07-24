using Project;

namespace Demo
{
    public class PoppingState:Statebase
    {
        public PoppingState(GameManger manger) : base(manger)
        {
        }

        public override void Enter(Block block)
        {
            base.Enter(block);
            block.State = BlockState.Popping;
        }

        public override void Update(Block block)
        {
            base.Update(block);
            //popingTimer计时结束
            // if (poptingTime == 0)
            // {
            //     Exit(block);
            // }
        }

        public override void Exit(Block block)
        {
            base.Exit(block);
            StateManger._instance.ChangeState(BlockState.Popped,block);
        }
    }
}
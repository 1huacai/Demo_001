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
            timerID = TimerMgr._Instance.Schedule(() =>
            {
                StateManger._instance.ChangeState(BlockState.Popped, block);
                Exit(block);
            }, ConstValues.poppingFps * ConstValues.fpsTime);

        }

        public override void Exit(Block block)
        {
            base.Exit(block);
        }
    }
}
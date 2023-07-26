using Project;

namespace Demo
{
    public class PoppingState : Statebase
    {
        public PoppingState(GameManger manger) : base(manger)
        {
        }

        public override void Enter(Block block)
        {
            if (block.Type == BlockType.None)
                return;
            block.State = BlockState.Popping;
        }

        public override void Update(Block block)
        {
            if (block.Type == BlockType.None)
                return;
            timerID = TimerMgr._Instance.Schedule(
                () => { StateManger._instance.ChangeState(BlockState.Popped, block); },
                ConstValues.poppingFps * ConstValues.fpsTime);
        }

        public override void Exit(Block block)
        {
            base.Exit(block);
        }
    }
}
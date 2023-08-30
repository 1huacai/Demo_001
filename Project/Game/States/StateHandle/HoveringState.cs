using Project;

namespace Demo
{
    public class HoveringState : Statebase
    {
        public HoveringState(Controller controller) : base(controller)
        {
        }

        public override void Enter(Block block)
        {
            if (block.Shape == BlockShape.None)
                return;
            block.State = BlockState.Hovering;
            block.CurStateFrame = ConstValues.hoveringFps;
        }

        public override void Update(Block block)
        {
            if (block.Shape == BlockShape.None)
                return;

            timerID = TimerMgr._Instance.Schedule(() =>
            {
                StateManger._instance.ChangeState(BlockState.Falling, block);
                
            }, ConstValues.hoveringFps * ConstValues.fpsTime);
        }
    }
}
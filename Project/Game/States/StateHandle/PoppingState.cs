using Project;
using UnityEngine;

namespace Demo
{
    public class PoppingState : Statebase
    {
        public PoppingState(SelfGameController controller) : base(controller)
        {
        }

        public override void Enter(Block block)
        {
            if(block == null)
                return;
            if (block.Shape == BlockShape.None)
                return;
            block.State = BlockState.Popping;
            block._Animation.PlayAnimation(string.Format("{0}_{1}",block.Shape,block.State),1,false);
        }

        public override void Update(Block block)
        {
            if (block.Shape == BlockShape.None)
                return;
            timerID = TimerMgr._Instance.Schedule(
                () =>
                {
                    block._Animation.StopAnimation();
                    StateManger._instance.ChangeState(BlockState.Popped, block);
                },
                ConstValues.poppingFps * ConstValues.fpsTime);
        }

        public override void Exit(Block block)
        {
            base.Exit(block);
        }
    }
}
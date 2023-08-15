using Project;
using UnityEngine;

namespace Demo
{
    public class HoveringState : Statebase
    {
        public HoveringState(SelfGameController controller) : base(controller)
        {
        }

        public override void Enter(Block block)
        {
            if (block.Type == BlockType.None)
                return;
            block.State = BlockState.Hovering;
        }

        public override void Update(Block block)
        {
            if (block.Type == BlockType.None)
                return;
            timerID = TimerMgr._Instance.Schedule(() =>
            {
                var downBlock = SelfGameController.blockMatrix[block.Row - 1, block.Col - 1];
                if (downBlock.Type == BlockType.None)
                {
                    StateManger._instance.ChangeState(BlockState.Falling, block);
                }
                else if (downBlock.Type != BlockType.None && downBlock.State == BlockState.Hovering)
                {
                    StateManger._instance.ChangeState(BlockState.Hovering, block);
                }
                else if (downBlock.Type != BlockType.None && downBlock.State == BlockState.Normal)
                {
                    StateManger._instance.ChangeState(BlockState.Landing, block);
                }
            }, ConstValues.hoveringFps * ConstValues.fpsTime);
        }
    }
}
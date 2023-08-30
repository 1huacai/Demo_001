using Project;
using UnityEngine;

namespace Demo
{
    public class FallingState : Statebase
    {
        public FallingState(Controller controller) : base(controller)
        {
        }

        public override void Enter(Block block)
        {
            if (block.Shape == BlockShape.None)
                return;
            block.State = BlockState.Falling;
            block.IsNeedFall = true;
            block.CurStateFrame = ConstValues.fallingFps;
        }

        public override void Update(Block block)
        {
            if (block.Shape == BlockShape.None)
                return;
            timerID = TimerMgr._Instance.Schedule(() =>
            {
                var downBlock = (_controller as SelfGameController).blockMatrix[block.Row - 1, block.Col - 1];
                if (block.Row == 1)
                {
                    StateManger._instance.ChangeState(BlockState.Landing, block);
                }
                else if (downBlock.Shape == BlockShape.None)
                {
                    StateManger._instance.ChangeState(BlockState.Falling, block);
                }
                else if (downBlock.Shape != BlockShape.None && downBlock.State == BlockState.Falling)
                {
                    StateManger._instance.ChangeState(BlockState.Falling, block);
                }
                else if (downBlock.Shape != BlockShape.None && downBlock.State == BlockState.Hovering)
                {
                    StateManger._instance.ChangeState(BlockState.Hovering, block);
                }
                else if (downBlock.Shape != BlockShape.None && downBlock.State == BlockState.Swapping)
                {
                    StateManger._instance.ChangeState(BlockState.Hovering, block);
                }
                else if (downBlock.Shape != BlockShape.None && downBlock.State == BlockState.Normal)
                {
                    StateManger._instance.ChangeState(BlockState.Landing, block);
                }
                else if (downBlock.Shape != BlockShape.None && downBlock.State == BlockState.Landing)
                {
                    StateManger._instance.ChangeState(BlockState.Landing, block);
                }else if (downBlock.Shape != BlockShape.None && downBlock.State == BlockState.Matched)
                {
                    StateManger._instance.ChangeState(BlockState.Falling, block);
                }
            }, ConstValues.fallingFps * ConstValues.fpsTime);
        }

        public override void Enter(PressureBlock pressureBlock)
        {
            base.Enter(pressureBlock);
            pressureBlock.State = BlockState.Falling;
            pressureBlock.IsNeedFall = true;
        }

        public override void Update(PressureBlock pressureBlock)
        {
            base.Update(pressureBlock);
            TimerMgr._Instance.Schedule(() =>
            {
                if (!pressureBlock.HasObstacleWithDown())
                {
                    StateManger._instance.ChangeState(BlockState.Falling, pressureBlock);
                }  
                else
                {
                    StateManger._instance.ChangeState(BlockState.Normal, pressureBlock);
                }
            }, ConstValues.fallingFps * ConstValues.fpsTime);
        }

        public override void Exit(PressureBlock block)
        {
            base.Exit(block);
        }
    }
}
using Project;
using UnityEngine;

namespace Demo
{
    public class FallingState:Statebase
    {
        public FallingState(GameManger manger) : base(manger)
        {
            
        }

        public override void Enter(Block block)
        {
            if (block.Type == BlockType.None)
                return;
            block.State = BlockState.Falling;
            block.IsNeedFall = true;
        }

        public override void Update(Block block)
        {
            if (block.Type == BlockType.None)
                return;
            timerID = TimerMgr._Instance.Schedule(() =>
            {
                Exit(block);
                var downBlock = _gameManger.blockMatrix[block.Row - 1, block.Col - 1];
                if (block.Row == 1)
                {
                    StateManger._instance.ChangeState(BlockState.Landing, block);
                }
                else if (downBlock.Type == BlockType.None)
                {
                    StateManger._instance.ChangeState(BlockState.Falling, block);
                }
                else if (downBlock.Type != BlockType.None && downBlock.State == BlockState.Falling)
                {
                    StateManger._instance.ChangeState(BlockState.Falling, block);
                }
                else if (downBlock.Type != BlockType.None && downBlock.State == BlockState.Hovering)
                {
                    StateManger._instance.ChangeState(BlockState.Hovering, block);
                }
                else if (downBlock.Type != BlockType.None && downBlock.State != BlockState.Hovering)
                {
                    StateManger._instance.ChangeState(BlockState.Landing, block);
                }else if (downBlock.Type != BlockType.None && downBlock.State == BlockState.Normal)
                {
                    StateManger._instance.ChangeState(BlockState.Landing, block);
                }
                
            }, ConstValues.fallingFps * ConstValues.fpsTime);
            
        }
        
    }
}
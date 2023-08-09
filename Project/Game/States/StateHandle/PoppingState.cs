using Project;
using UnityEngine;

namespace Demo
{
    public class PoppingState : Statebase
    {
        public PoppingState(GameManger manger) : base(manger)
        {
        }

        public override void Enter(Block block)
        {
            if(block == null)
                return;
            if (block.Type == BlockType.None)
                return;
            block.State = BlockState.Popping;
            block._Animation.PlayAnimation(string.Format("{0}_{1}",block.Type,block.State),2,false);
        }

        public override void Update(Block block) 
        {
            if (block.Type == BlockType.None)
                return;
            timerID = TimerMgr._Instance.Schedule(
                () =>
                {
                    block._Animation.stop = true;
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
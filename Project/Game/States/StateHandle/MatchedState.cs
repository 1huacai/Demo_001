using Project;
using UnityEngine;

namespace Demo
{
    public class MatchedState : Statebase
    {
        public MatchedState(SelfGameController controller) : base(controller)
        {
        }

        public override void Enter(Block block)
        {
            if (block.Type == BlockType.None)
                return;
            block.State = BlockState.Matched;
            block._Animation.PlayAnimation(string.Format("{0}_{1}",block.Type,block.State));
        }

        public override void Update(Block block)
        {
            if (block.Type == BlockType.None)
                return;
            TimerMgr._Instance.Schedule(() => { Exit(block); }, ConstValues.matchedFps * ConstValues.fpsTime);
        }

        public override void Exit(Block block)
        {
            base.Exit(block);
            StateManger._instance.ChangeState(BlockState.Popping, block);
            block._Animation.stop = true;
        }
    }
}
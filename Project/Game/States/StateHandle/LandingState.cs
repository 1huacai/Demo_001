using System.Collections.Generic;
using Project;
using UnityEngine;

namespace Demo
{
    public class LandingState : Statebase
    {
        public LandingState(Controller controller) : base(controller)
        {
        }

        public override void Enter(Block block)
        {
            if (block.Shape == BlockShape.None)
                return;
            block.State = BlockState.Landing;
            block._Animation.PlayAnimation(string.Format("{0}_{1}",block.Shape,block.State));
            block.CurStateFrame = ConstValues.landingFps;
            
            var otherBlock = OtherGameController.Inst.blockMatrix[block.Row, block.Col - 1];
            otherBlock._Animation.PlayAnimation(string.Format("{0}_{1}",block.Shape,block.State));
        }

        public override void Update(Block block)
        {
            if (block.Shape == BlockShape.None)
                return;
            timerID = TimerMgr._Instance.Schedule(() =>
            {
                block._Animation.StopAnimation(()=>{block.ResetOriginImg();});
                var otherBlock = OtherGameController.Inst.blockMatrix[block.Row, block.Col - 1];
                otherBlock._Animation.StopAnimation(()=>{otherBlock.ResetOriginImg();});
                
                //横向和纵向没有可消除的相邻block
                var selfController = _controller as SelfGameController;
                var sameBlocks = selfController.GetSameBlocksWith(block);
                if (sameBlocks.Count < 3)
                {
                    block.Chain = false;
                    StateManger._instance.ChangeState(BlockState.Normal, block);
                }
                else
                {
                    if(!selfController.IsBlockInSameFrame(block))
                        selfController.BlocksInSameFrame.Add(block);
                }
            }, ConstValues.landingFps * ConstValues.fpsTime);
        }
    }
}
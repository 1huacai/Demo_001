using System.Collections.Generic;
using Project;
using UnityEngine;

namespace Demo
{
    public class LandingState : Statebase
    {
        public LandingState(SelfGameController controller) : base(controller)
        {
        }

        public override void Enter(Block block)
        {
            if (block.Shape == BlockShape.None)
                return;
            block.State = BlockState.Landing;
            block._Animation.PlayAnimation(string.Format("{0}_{1}",block.Shape,block.State),2,false);
        }

        public override void Update(Block block)
        {
            if (block.Shape == BlockShape.None)
                return;
            timerID = TimerMgr._Instance.Schedule(() =>
            {
                //横向和纵向没有可消除的相邻block
                var sameBlocks = SelfGameController.GetSameBlocksWith(block);
                if (sameBlocks.Count < 3)
                {
                    block.Chain = false;
                    StateManger._instance.ChangeState(BlockState.Normal, block);
                }
                else
                {
                    Debug.LogError("进入Landing待转matched");
                    SelfGameController.BlocksInSameFrame.Add(sameBlocks);
                    
                    //所有相同的棋子都要变为matched状态
                    for (int i = 0; i < sameBlocks.Count; i++)
                    {
                        var targetBlock = sameBlocks[i];
                        Debug.LogError($"{targetBlock.name}-{targetBlock.Shape}-{sameBlocks.Count}");
                        StateManger._instance.ChangeState(BlockState.Matched, targetBlock);
                        //设置该棋子上方的棋子chain为true
                        SelfGameController.SetUpRowBlockChain(targetBlock);
                    }
                    

                }
            }, ConstValues.landingFps * ConstValues.fpsTime);
        }
    }
}
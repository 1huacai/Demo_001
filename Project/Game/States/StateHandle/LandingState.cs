﻿using System.Collections.Generic;
using Project;

namespace Demo
{
    public class LandingState:Statebase
    {
        public LandingState(GameManger manger) : base(manger)
        {
            
        }

        public override void Enter(Block block)
        {
            base.Enter(block);
            block.State = BlockState.Landing;
        }

        public override void Update(Block block)
        {
            base.Update(block);
            timerID = TimerMgr._Instance.Schedule(() =>
            {
                //横向和纵向没有可消除的相邻block
                var sameBlocks = _gameManger.GetSameBlocksWith(block);
                if (sameBlocks.Count < 3)
                {
                    StateManger._instance.ChangeState(BlockState.Normal,block);
                }
                else
                {
                    //所有相同的棋子都要变为matched状态
                    for (int i = 0; i < sameBlocks.Count; i++)
                    {
                        var targetBlock = sameBlocks[i];
                        StateManger._instance.ChangeState(BlockState.Matched,targetBlock);
                    }
                }
                Exit(block);
            },ConstValues.landingFps * ConstValues.fpsTime);
           
        }

        public override void Exit(Block block)
        {
            base.Exit(block);
        }
    }
}
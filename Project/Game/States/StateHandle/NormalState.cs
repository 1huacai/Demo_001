﻿using Project;
using UnityEngine;

namespace Demo
{
    public class NormalState : Statebase
    {
        public NormalState(SelfGameController controller) : base(controller)
        {
        }

        public override void Enter(Block block)
        {
            if (block.Type == BlockType.None)
                return;
            block.State = BlockState.Normal;
            
            var sameBlocks = SelfGameController.GetSameBlocksWith(block);
            if (sameBlocks.Count >= 3)//有可以匹配消除的block
            {
                Debug.LogError("进入normal待转matched");
                SelfGameController.BlocksInSameFrame.Add(sameBlocks);
                
                for (int i = 0; i < sameBlocks.Count; i++)
                {  
                    var targetBlock = sameBlocks[i];
                    Debug.LogError($"{targetBlock.name}-{targetBlock.Type}-{sameBlocks.Count}");
                    StateManger._instance.ChangeState(BlockState.Matched,targetBlock);
                    //设置上方的棋子chain为true
                    SelfGameController.SetUpRowBlockChain(targetBlock);
                }
            }
        } 

        public override void Update(Block block)
        {
            if (block.Type == BlockType.None)
                return;
            //下方棋子Type为None且block的row = 1
            if (block.Row > 1)
            {
                var downBlock = SelfGameController.blockMatrix[block.Row - 1, block.Col - 1];
                if (downBlock.Type == BlockType.None && !SelfGameController.Inst.CheckPressureBlockIncludeBlock(downBlock))
                {
                    StateManger._instance.ChangeState(BlockState.Hovering, block);
                }
            }
            
        }


        public override void OnBlockOperation(int row, int col, BlockOperation operation)
        {
            base.OnBlockOperation(row, col, operation);
            var block = SelfGameController.blockMatrix[row, col - 1];
            if (operation == BlockOperation.TouchDown)
            {
                block.IsSelected = true;
            }

            if (operation == BlockOperation.TouchUp)
            {
                block.IsSelected = false;
            }
        }

        public override void Enter(PressureBlock pressureBlock)
        {
            base.Enter(pressureBlock);
            pressureBlock.State = BlockState.Normal;
        }

        public override void Update(PressureBlock pressureBlock)
        {
            base.Update(pressureBlock);
            if (!pressureBlock.HasObstacleWithDown())
            {
                StateManger._instance.ChangeState(BlockState.Falling,pressureBlock);
            }
            
        }

        public override void Exit(PressureBlock block)
        {
            base.Exit(block);
        }
    }
}
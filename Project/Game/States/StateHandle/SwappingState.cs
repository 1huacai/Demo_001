﻿using Project;
using DG.Tweening;
using UnityEngine;

namespace Demo
{
    public class SwappingState:Statebase
    {
        public SwappingState(GameManger manger) : base(manger)
        {
            
        }

        public override void Enter(Block block)
        {
            base.Enter(block);
            block.State = BlockState.Swapping;
        }

        public override void Update(Block block)
        {
            base.Update(block);
            
            
            
        }

        public override void Exit(Block block)
        {
            base.Exit(block);
        }
        
        public override void OnBlockOperation(int row, int col, BlockOperation operation)
        {
            base.OnBlockOperation(row, col, operation);
            if (operation == BlockOperation.DragHalf)
            {
                if(_gameManger.selectBlock == null)
                    return;
                var otherBlock = _gameManger.blockMatrix[row, col - 1];
                if (otherBlock == null || _gameManger.selectBlock.State == BlockState.Swapping)
                    return;
                DoSwap(_gameManger.selectBlock, otherBlock);
            }
        }
        
       
        private void DoSwap(Block block_1, Block block_2)
        {
            var block_1_Pos = block_1.dragBeginPos;
            var block_2_Pos = block_2.transform.localPosition;
            
            // block_1.State = BlockState.Swapping;
            // block_2.State = BlockState.Swapping;
            StateManger._instance.ChangeStageEnter(BlockState.Swapping,block_1,block_2);
            
            //表现部分
            block_1.transform.DOLocalMove(block_2_Pos, 10 * Time.deltaTime).OnComplete(() => { });
            block_2.transform.DOScale(0.9f, 5 * Time.deltaTime);
            block_2.transform.DOLocalMove(block_1_Pos, 10 * Time.deltaTime).OnComplete(() =>
            {
                block_2.transform.DOScale(ConstValues.BLOCK_BIG_SCALE, 5 * Time.deltaTime).OnComplete(() =>
                {
                    block_2.transform.DOScale(1f, 5 * Time.deltaTime).OnComplete((() =>
                    {   
                        //swaping 状态下的状态更新
                        //StateManger._instance.ChangeStageUpdate(BlockState.Swapping,block_1,block_2);
                        StateManger._instance.ChangeStageEnter(BlockState.Normal,block_1,block_2);
                    }));
                });
            });
            //数据部分
            block_1.dragBeginPos = block_2_Pos;
            _gameManger.blockMatrix[block_1.Row, block_1.Col - 1] = block_2;
            _gameManger.blockMatrix[block_2.Row, block_2.Col - 1] = block_1;
            int tempRow = block_1.Row;
            int tempCol = block_1.Col;

            block_1.Row = block_2.Row;
            block_1.Col = block_2.Col;
            block_1.ChangeBlockObjName();

            block_2.Row = tempRow;
            block_2.Col = tempCol;
            block_2.ChangeBlockObjName();
        }
    }
}
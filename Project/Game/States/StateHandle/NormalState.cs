using Project;
using UnityEngine;

namespace Demo
{
    public class NormalState : Statebase
    {
        public NormalState(Controller controller) : base(controller)
        {
        }

        public override void Enter(Block block)
        {
            if (block.Shape == BlockShape.None)
                return;
            block.State = BlockState.Normal;
        }

        public override void Update(Block block)
        {
            if (block.Shape == BlockShape.None)
                return;
            var selfController = _controller as SelfGameController;
            var sameBlocks = selfController.GetSameBlocksWith(block);
            if (sameBlocks.Count >= 3) //有可以匹配消除的block
            {
                if(!selfController.IsBlockInSameFrame(block))
                    selfController.BlocksInSameFrame.Add(block);
                return;
            }

            // //下方棋子Type为None且block的row = 1
            if (block.Row > 1)
            {
                //改变空棋子上方所有棋子的状态为Hovering
                var downBlock = selfController.blockMatrix[block.Row - 1, block.Col - 1];
                if (downBlock.Shape == BlockShape.None &&
                    !selfController.CheckPressureBlockIncludeBlock(downBlock))
                {
                    int row = downBlock.Row + 1;
                    int col = downBlock.Col - 1;
                    for (int i = row; i < ConstValues.MAX_ROW; i++)
                    {
                        var t_block = selfController.blockMatrix[i, col];
                        if (t_block.Shape == BlockShape.None || t_block.State == BlockState.Hovering)
                            break;
                        StateManger._instance.ChangeState(BlockState.Hovering, t_block);
                    }
                }
            }
        }


        public override void OnBlockOperation(int row, int col, BlockOperation operation)
        {
            base.OnBlockOperation(row, col, operation);
            var block = (_controller as SelfGameController)?.blockMatrix[row, col - 1];
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
                StateManger._instance.ChangeState(BlockState.Falling, pressureBlock);
            }
        }

        public override void Exit(PressureBlock block)
        {
            base.Exit(block);
        }
    }
}
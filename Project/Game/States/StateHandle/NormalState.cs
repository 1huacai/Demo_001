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
            
            var sameBlocks = (_controller as SelfGameController)?.GetSameBlocksWith(block);
            if (sameBlocks.Count >= 3)//有可以匹配消除的block
            {
                Debug.LogError("进入normal待转matched");
                (_controller as SelfGameController)?.BlocksInSameFrame.Add(sameBlocks);
                //单人模式下走本地的匹配逻辑
                if (!NetManager.Instance.Multiplayer)
                {
                    for (int i = 0; i < sameBlocks.Count; i++)
                    {
                        var targetBlock = sameBlocks[i];
                        Debug.LogError($"{targetBlock.name}-{targetBlock.Shape}-{sameBlocks.Count}");
                        StateManger._instance.ChangeState(BlockState.Matched, targetBlock);
                        //设置上方的棋子chain为true
                        (_controller as SelfGameController)?.SetUpRowBlockChain(targetBlock);
                    }
                }
            }
        } 

        public override void Update(Block block)
        {
            if (block.Shape == BlockShape.None)
                return;
            //下方棋子Type为None且block的row = 1
            if (block.Row > 1)
            {
                var downBlock = (_controller as SelfGameController)?.blockMatrix[block.Row - 1, block.Col - 1];
                if (downBlock.Shape == BlockShape.None && !SelfGameController.Inst.CheckPressureBlockIncludeBlock(downBlock))
                {
                    StateManger._instance.ChangeState(BlockState.Hovering, block);
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
                StateManger._instance.ChangeState(BlockState.Falling,pressureBlock);
            }
            
        }

        public override void Exit(PressureBlock block)
        {
            base.Exit(block);
        }
    }
}
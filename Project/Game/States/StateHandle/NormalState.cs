using Project;
using UnityEngine;

namespace Demo
{
    public class NormalState : Statebase
    {
        public NormalState(GameManger manger) : base(manger)
        {
        }

        public override void Enter(Block block)
        {
            if (block.Type == BlockType.None)
                return;
            block.State = BlockState.Normal;
            
            var sameBlocks = _gameManger.GetSameBlocksWith(block);
            if (sameBlocks.Count >= 3)//有可以匹配消除的block
            {
                Debug.LogError("进入normal待转matched");
                if (sameBlocks.Count >= 4)
                {
                    //combo效果
                    _gameManger.GenComboObj(sameBlocks.Count,sameBlocks[0].transform.localPosition);
                }
                
                for (int i = 0; i < sameBlocks.Count; i++)
                {  
                    var targetBlock = sameBlocks[i];
                    Debug.LogError($"{targetBlock.name}-{targetBlock.Type}-{sameBlocks.Count}");
                    StateManger._instance.ChangeState(BlockState.Matched,targetBlock);
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
                var downBlock = _gameManger.blockMatrix[block.Row - 1, block.Col - 1];
                if (downBlock.Type == BlockType.None)
                {
                    StateManger._instance.ChangeState(BlockState.Hovering, block);
                    return;
                }
            }
            
        }


        public override void OnBlockOperation(int row, int col, BlockOperation operation)
        {
            base.OnBlockOperation(row, col, operation);
            var block = _gameManger.blockMatrix[row, col - 1];
            if (operation == BlockOperation.TouchDown)
            {
                block.IsSelected = true;
            }

            if (operation == BlockOperation.TouchUp)
            {
                block.IsSelected = false;
            }
        }
    }
}
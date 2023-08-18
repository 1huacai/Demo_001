using Project;
using DG.Tweening;
using UnityEngine;

namespace Demo
{
    public class SwappingState : Statebase
    {
        public SwappingState(Controller controller) : base(controller)
        {
        }

        public override void Enter(Block block)
        {
            if (block.Shape == BlockShape.None)
                return;

            block.State = BlockState.Swapping;
        }

        public override void Update(Block block)
        {
            if (block.Shape == BlockShape.None)
                return;
            if (block.Row == 1)
            {
                StateManger._instance.ChangeState(BlockState.Normal, block);
                return;
            }

            //获取在当前block下面的格子的状态
            var downBlock = (_controller as SelfGameController)?.blockMatrix[block.Row - 1, block.Col - 1];
            var downBlockState = downBlock.State;
            if (downBlock.Shape != BlockShape.None && downBlockState != BlockState.Hovering &&
                downBlockState != BlockState.Falling)
            {
                StateManger._instance.ChangeStageEnter(BlockState.Normal, block);
            }
            else if (downBlock.Shape == BlockShape.None && !SelfGameController.Inst.CheckPressureBlockIncludeBlock(downBlock))
            {
                StateManger._instance.ChangeState(BlockState.Hovering, block);
            }
            else if (downBlock.Shape == BlockShape.None && SelfGameController.Inst.CheckPressureBlockIncludeBlock(downBlock))
            {
                StateManger._instance.ChangeStageEnter(BlockState.Normal, block);
            }
            else if (downBlock.Shape != BlockShape.None && downBlockState == BlockState.Hovering)
            {
                StateManger._instance.ChangeState(BlockState.Hovering, block);
            }
            else if (downBlock.Shape != BlockShape.None && downBlockState != BlockState.Hovering &&
                     downBlockState == BlockState.Falling)
            {
                StateManger._instance.ChangeState(BlockState.Hovering, block);
            }
        }

        public override void OnBlockOperation(int row, int col, BlockOperation operation)
        {
            base.OnBlockOperation(row, col, operation);
            if (operation == BlockOperation.DragHalf)
            {
                if ((_controller as SelfGameController)?.selectBlock == null)
                    return;
                var otherBlock = (_controller as SelfGameController)?.blockMatrix[row, col - 1];

                if ((_controller as SelfGameController)?.selectBlock.State != BlockState.Normal ||
                    otherBlock.State != BlockState.Normal ||
                    (_controller as SelfGameController).CheckPressureBlockIncludeBlock(otherBlock))
                {
                    return;
                }

                if (NetManager.Instance.Multiplayer)
                {
                    Debug.LogError("开始交换请求");
                    NetManager.Instance.GameSwapReq(TimerMgr._Instance.Frame, (_controller as SelfGameController)?.selectBlock, otherBlock,true,DoSwap);
                }
                else
                {
                    var selectBlock = (_controller as SelfGameController)?.selectBlock;
                    DoSwap(selectBlock, otherBlock,true);
                    
                    //对手镜像交换
                    var otherController = OtherGameController.Inst;
                    int row_s = selectBlock.Row;
                    int col_s = selectBlock.Col;
                    int row_o = otherBlock.Row;
                    int col_o = otherBlock.Col;
                    var selectBlock_Other = otherController.blockMatrix[row_s, col_s - 1];
                    var otherBlock_Other = otherController.blockMatrix[row_o, col_o - 1];
                    DoSwap(selectBlock_Other, otherBlock_Other,false);
                }
            }
        }


        private void DoSwap(Block block_1, Block block_2,bool isSelf = false)
        {
            Debug.LogError($"{block_1.Row}-{block_1.Col}");
            Debug.LogError($"{block_2.Row}-{block_2.Col}");
            
            // var block_1_Pos = isSelf ? block_1.dragBeginPos : block_1.transform.localPosition;
            var block_1_Pos = block_1.transform.localPosition;
            var block_2_Pos = block_2.transform.localPosition;

            // block_1.State = BlockState.Swapping;
            // block_2.State = BlockState.Swapping;
            if(isSelf)
                StateManger._instance.ChangeStageEnter(BlockState.Swapping, block_1, block_2);

            //表现部分
            block_1.transform.DOLocalMove(block_2_Pos, 10 * ConstValues.fpsTime).OnComplete(() => { });
            block_2.transform.DOScale(0.9f, 5 * ConstValues.fpsTime);
            block_2.transform.DOLocalMove(block_1_Pos, 10 * ConstValues.fpsTime).OnComplete(() =>
            {
                block_2.transform.DOScale(ConstValues.BLOCK_BIG_SCALE, 5 * ConstValues.fpsTime).OnComplete(() =>
                {
                    block_2.transform.DOScale(1f, 5 * ConstValues.fpsTime).OnComplete((() =>
                    {
                        //swaping 状态下的状态更新 
                        if(isSelf)
                            StateManger._instance.ChangeStageUpdate(BlockState.Swapping, block_1, block_2);
                        //StateManger._instance.ChangeStageEnter(BlockState.Normal,block_1,block_2);
                    }));
                });
            });

            if (isSelf)
            {
                //数据部分,玩家自己数据交换
                block_1.dragBeginPos = block_2_Pos;
                ((SelfGameController) _controller).blockMatrix[block_1.Row, block_1.Col - 1] = block_2;
                ((SelfGameController) _controller).blockMatrix[block_2.Row, block_2.Col - 1] = block_1;
                int tempRow = block_1.Row;
                int tempCol = block_1.Col;

                block_1.Row = block_2.Row;
                block_1.Col = block_2.Col;
                block_1.ChangeBlockObjName();

                block_2.Row = tempRow;
                block_2.Col = tempCol;
                block_2.ChangeBlockObjName();
            }
            else
            {
                if (!NetManager.Instance.Multiplayer)
                {
                    //敌方镜像数据交换
                    block_1.dragBeginPos = block_2_Pos;
                    OtherGameController.Inst.blockMatrix[block_1.Row, block_1.Col - 1] = block_2;
                    OtherGameController.Inst.blockMatrix[block_2.Row, block_2.Col - 1] = block_1;
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
    }
}
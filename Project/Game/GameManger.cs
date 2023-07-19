using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Project;
using FrameWork.Manager;
using ResourceLoad;
using Random = UnityEngine.Random;

namespace Demo
{
    public class GameManger
    {
        private static GameManger s_inst;
        public static GameManger Inst
        {
            get
            {
                if (s_inst == null) s_inst = new GameManger();
                return s_inst;
            }
        }

        public Block selectBlock;
        
        public GameManger()
        {
            Application.targetFrameRate = 60;
        }
        
        //自身所有棋子的初始化数据
        private List<BlockData> blockDatas = new List<BlockData>();
        public Block[,] blockMatrix = new Block[ConstValues.MAX_MATRIX_ROW, ConstValues.MAX_COL];
        
        
        /// <summary>
        /// 初始化创建所有blockData数据并返回
        /// </summary>
        /// <returns></returns>
        public List<BlockData> GenBlockDatas()
        {
            blockDatas.Clear();
            int maxGenRow = Random.Range(ConstValues.MiN_GENROW, ConstValues.MAX_GENROW + 1);
            for (int row = 1; row <= maxGenRow; row++)
            {
                for (int col = 1; col <= ConstValues.MAX_COL; col++)
                {
                    //具体的blockdata数据
                    BlockType type = (BlockType) Random.Range(0, ConstValues.MAX_BLOCKTYPE);
                    BlockData newData = new BlockData(row, col, type);
                    blockDatas.Add(newData);
                }
            }

            return blockDatas;
        }

        /// <summary>
        /// 构建所有棋子
        /// </summary>
        /// <param name="datas"></param>
        /// <param name="boardTran"></param>
        public void GenBlocks(List<BlockData> datas, Transform boardTran)
        {
            SingletonManager.GetManager<ResourcesManager>().LoadPrefab($"Prefabs/Block", (obj, l) =>
            {
                if (null == obj)
                {
                    return;
                }
                
                //遍历所有数据新建棋子
                for (int i = 0; i < datas.Count; i++)
                {
                    var data = datas[i];
                    int row = data.row;
                    int col = data.col;
                    BlockType type = data.type;
                    
                    // //空方块不生成对象
                    // if(type == BlockType.None)
                    //     continue;
                    
                    Block block = Block.CreateBlockObject(obj, row, col, type, boardTran,this);
                    //设置棋子位置
                    block.transform.localPosition = new Vector3(
                        ConstValues.BLOCK_X_ORIGINPOS + (col - 1) * ConstValues.BLOCK_X_OFFSET,
                        ConstValues.BLOCK_Y_ORIGINPOS + (row - 1) * ConstValues.BLOCK_Y_OFFSET,
                        0f
                    );
                    // Debug.LogError(row + "-" + col);
                    // Debug.LogError(row + "-" + col);
                    blockMatrix[row, col - 1] = block;
                    block.BlockOperationEvent += OnBlockOperation;
                }
            });
        }

        private void OnBlockOperation(int row, int col, BlockOperation operation)
        {   
            //执行拖拽操作
            if (operation == BlockOperation.DragHalf)
            {   
                //待交换的block
                var otherBlock = blockMatrix[row, col - 1];
                if(otherBlock == null || swaping)
                    return;
                DoSwap(selectBlock, otherBlock);
            }
        }

        public bool swaping = false;

        private void DoSwap(Block block_1, Block block_2)
        {
            var block_1_Pos = block_1.dragBeginPos;
            var block_2_Pos = block_2.transform.localPosition;

            swaping = true;
            block_1.transform.DOLocalMove(block_2_Pos, 10 * Time.deltaTime).OnComplete(() =>
            {
                block_1.dragBeginPos = block_1.transform.localPosition;
            });
            block_2.transform.DOScale(0.9f, 5 * Time.deltaTime);
            block_2.transform.DOLocalMove(block_1_Pos, 10 * Time.deltaTime).OnComplete(() =>
            {
                block_2.transform.DOScale(ConstValues.BLOCK_BIG_SCALE, 5 * Time.deltaTime);
                block_2.transform.DOScale(1f, 5 * Time.deltaTime);

                blockMatrix[block_1.Row, block_1.Col - 1] = block_2;
                blockMatrix[block_2.Row, block_2.Col - 1] = block_1;
                int tempRow = block_1.Row;
                int tempCol = block_1.Col;

                block_1.Row = block_2.Row;
                block_1.Col = block_2.Col;
                block_1.ChangeBlockObjName();

                block_2.Row = tempRow;
                block_2.Col = tempCol;
                block_2.ChangeBlockObjName();
                swaping = false;
            });
        }
    }
}

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
        public Block[,] blockMatrix = new Block[ConstValues.MAX_MATRIX_ROW, ConstValues.MAX_COL];

        #region 初始化blockStageData部分
        //所有stage的样式的配置
        List<string[]> stageConfigs = new List<string[]>
        {
            ConstValues.stage_1, ConstValues.stage_2
        };
        /// <summary>
        /// 初始化创建所有blockData数据并返回
        /// </summary>
        /// <returns></returns>
        public List<BlockData> GenBlockDatas(int speedLevel = 1)
        {
            int maxRow = ConstValues.MAX_GENROW, maxCol = ConstValues.MAX_COL;
            BlockData[,] blockDataMatrix = new BlockData[maxRow, maxCol];
            List<BlockData> blockDataList = new List<BlockData>();

            for (int row = 0; row < maxRow; row++)
            {
                for (int col = 0; col < maxCol; col++)
                {
                    BlockType type = BlockType.None;

                    if (blockDataList.Count < 30)
                    {
                        do
                        {
                            type = (BlockType) Random.Range(1, 6); // Randomly generate a BlockType that is not None
                        } while ((speedLevel > 3 && IsSameAsAdjacent(blockDataMatrix, row, col, type))
                                 || (row >= 2 && blockDataMatrix[row - 1, col].type == type &&
                                     blockDataMatrix[row - 2, col].type == type)
                                 || (col >= 2 && blockDataMatrix[row, col - 1].type == type &&
                                     blockDataMatrix[row, col - 2].type == type));
                    }
                    else if (row > 0 && blockDataMatrix[row - 1, col].type == BlockType.None)
                    {
                        type = BlockType.None;
                    }

                    blockDataMatrix[row, col] = new BlockData(row + 1, col + 1, type);
                    blockDataList.Add(blockDataMatrix[row, col]);
                }
            }

            if (Random.Range(0, 2) != 0)
            {
                blockDataList = MatchStageBlockDatas(blockDataList,stageConfigs[Random.Range(0,stageConfigs.Count)]);
            }

            return blockDataList;
        }

        private bool IsSameAsAdjacent(BlockData[,] blockDataMatrix, int row, int col, BlockType type)
        {
            return (row > 0 && blockDataMatrix[row - 1, col].type == type) ||
                   (col > 0 && blockDataMatrix[row, col - 1].type == type);
        }

        //根据模板随机选择地图沟壑配置
        private List<BlockData> MatchStageBlockDatas(List<BlockData> blockDatas,string[] stage)
        {
            List<int> startChangeIndexs = new List<int>(); //起始需要交换的位置
            List<int> endChangeIndexs = new List<int>(); //结束需要交换的位置
            for (int row = ConstValues.MAX_GENROW - 1; row >= 0; row--)
            {
                for (int col = 0; col < ConstValues.MAX_COL; col++)
                {
                    int index = row * 6 + col;
                    int indexOfBlockDatas = (ConstValues.MAX_GENROW - 1 - row) * ConstValues.MAX_COL + col;

                    if (stage[index].Equals("0") && row >= 1)
                    {
                        startChangeIndexs.Add(indexOfBlockDatas);
                    }
                    else if (stage[index].Equals("c"))
                    {
                        endChangeIndexs.Add(indexOfBlockDatas);
                    }
                }
            }

            for (int i = 0; i < startChangeIndexs.Count; i++)
            {
                var blockData_1 = blockDatas[startChangeIndexs[i]];
                var blockData_2 = blockDatas[endChangeIndexs[i]];
                BlockType temp = blockData_1.type;
                blockData_1.type = blockData_2.type;
                blockData_2.type = temp;
            }

            return blockDatas;
        }

        //获取与之前不同的blocktype
        private BlockType GetDiffTypeFrom(params BlockType[] oldTypes)
        {
            BlockType newType = BlockType.None;
            while (Array.IndexOf(oldTypes, newType) > 0)
            {
                newType = (BlockType) Random.Range(0, ConstValues.MAX_BLOCKTYPE);
            }

            return newType;
        }

        #endregion

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

                    Block block = Block.CreateBlockObject(obj, row, col, type, boardTran, this);
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
                if (otherBlock == null || swaping)
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
            //表现部分
            block_1.transform.DOLocalMove(block_2_Pos, 10 * Time.deltaTime).OnComplete(() => { });
            block_2.transform.DOScale(0.9f, 5 * Time.deltaTime);
            block_2.transform.DOLocalMove(block_1_Pos, 10 * Time.deltaTime).OnComplete(() =>
            {
                block_2.transform.DOScale(ConstValues.BLOCK_BIG_SCALE, 5 * Time.deltaTime).OnComplete(() =>
                {
                    block_2.transform.DOScale(1f, 5 * Time.deltaTime).OnComplete((() => { swaping = false; }));
                });
            });
            //数据部分
            block_1.dragBeginPos = block_2_Pos;
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
        }
    }
}
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
                if (s_inst == null)
                {
                    s_inst = new GameManger();
                }
                return s_inst;
            }
        }

        public Block selectBlock;
        public bool gameStart = false;//游戏开始标志
        
        
        public GameManger()
        {
           
        }
        
        //初始化游戏
        public void InitGame()
        {
            Application.targetFrameRate = ConstValues.targetPlatformFps;
            var gameView = UIManager.Inst.GetUI<GameView>(UIDef.GameView);
            var blockDatas = GameManger.Inst.GenBlockDatas();
            //根据数据构建所有棋子obj
            GenBlocks(blockDatas,gameView.BlockBoard);
            gameStart = true;
            StateManger._instance.Init(this);
            TimerMgr._Instance.Init();
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
                    BlockType Type = BlockType.None;

                    if (blockDataList.Count < 30)
                    {
                        do
                        {
                            Type = (BlockType) Random.Range(1, 6); // Randomly generate a BlockType that is not None
                        } while ((speedLevel > 3 && IsSameAsAdjacent(blockDataMatrix, row, col, Type))
                                 || (row >= 2 && blockDataMatrix[row - 1, col].type == Type &&
                                     blockDataMatrix[row - 2, col].type == Type)
                                 || (col >= 2 && blockDataMatrix[row, col - 1].type == Type &&
                                     blockDataMatrix[row, col - 2].type == Type));
                    }
                    else if (row > 0 && blockDataMatrix[row - 1, col].type == BlockType.None)
                    {
                        Type = BlockType.None;
                    }

                    blockDataMatrix[row, col] = new BlockData(row + 1, col + 1, Type);
                    blockDataList.Add(blockDataMatrix[row, col]);
                }
            }

            if (Random.Range(0, 2) != 0)
            {
                blockDataList = MatchStageBlockDatas(blockDataList,stageConfigs[Random.Range(0,stageConfigs.Count)]);
            }

            return blockDataList;
        }

        private bool IsSameAsAdjacent(BlockData[,] blockDataMatrix, int row, int col, BlockType Type)
        {
            return (row > 0 && blockDataMatrix[row - 1, col].type == Type) ||
                   (col > 0 && blockDataMatrix[row, col - 1].type == Type);
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

        //获取与之前不同的blockType
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
                    BlockType Type = data.type;

                    // //空方块不生成对象
                    // if(Type == BlockType.None)
                    //     continue;

                    Block block = Block.CreateBlockObject(obj, row, col, Type, boardTran, this);
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
                StateManger._instance.StateHandlers[BlockState.Swapping].OnBlockOperation(row,col,operation);
            }

            if (operation == BlockOperation.TouchUp || operation == BlockOperation.TouchDown)
            {
                StateManger._instance.StateHandlers[BlockState.Normal].OnBlockOperation(row,col,operation);
            }
        }

        public void FiexdUpdate()
        {
            if (!gameStart)
                return;

            UpDateBlockArea();
        }
        
        //更新棋盘区域逻辑
        private void UpDateBlockArea()
        {
            for (int row = 0; row < ConstValues.MAX_MATRIX_ROW; row++)
            {
                for (int col = 0; col < ConstValues.MAX_COL; col++)
                {
                    var block = blockMatrix[row, col];
                    if (block != null)
                    {
                        block.LogicUpdate();
                    }
                }
            }
        }

        /// <summary>
        /// 获取当前block在横向纵向上与自己相邻的相同Type(非None)的block
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public List<Block> GetSameBlocksWith(Block block)
        {
            //纵向上相邻且相同Type的集合
            List<Block> v_blocks = new List<Block>();
            //横向上相邻且相同Type的集合
            List<Block> h_blocks = new List<Block>();
            List<Block> sameBlocks = new List<Block>();

            //向上找
            int curRow = block.Row;
            int curCol = block.Col - 1;
            for (int row = curRow; row < ConstValues.MAX_ROW - 1; row++)
            {
                var targetBlock = blockMatrix[row + 1, curCol];
                var curBlock = blockMatrix[row, curCol];
                if (targetBlock.Type == BlockType.None)
                    break;
                if ((curBlock.Type == targetBlock.Type) 
                    && (curBlock.Type == block.Type)
                    && (targetBlock.State == BlockState.Normal ||
                        targetBlock.State == BlockState.Landing))
                {
                    v_blocks.Add(targetBlock);
                }
            }

            //向下找
            for (int row = curRow; row > 1; row--)
            {
                var targetBlock = blockMatrix[row - 1, curCol];
                var curBlock = blockMatrix[row, curCol];
                if (targetBlock.Type == BlockType.None)
                    break;
                if ((curBlock.Type == targetBlock.Type) 
                    && (curBlock.Type == block.Type)
                    && (targetBlock.State == BlockState.Normal ||
                        targetBlock.State == BlockState.Landing))
                {
                    v_blocks.Add(targetBlock);
                }
            }

            //向左找
            for (int col = curCol; col > 0; col--)
            {
                var targetBlock = blockMatrix[curRow, col - 1];
                var curBlock = blockMatrix[curRow, col];
                if (targetBlock.Type == BlockType.None)
                    break;
                if ((curBlock.Type == targetBlock.Type) 
                    && (curBlock.Type == block.Type)
                    && (targetBlock.State == BlockState.Normal ||
                        targetBlock.State == BlockState.Landing))
                {
                    h_blocks.Add(targetBlock);
                }
            }

            //向右找
            for (int col = curCol; col < ConstValues.MAX_COL - 1; col++)
            {
                var targetBlock = blockMatrix[curRow, col + 1];
                var curBlock = blockMatrix[curRow, col];
                if (targetBlock.Type == BlockType.None)
                    break;
                if ((curBlock.Type == targetBlock.Type) 
                    && (curBlock.Type == block.Type)
                    && (targetBlock.State == BlockState.Normal ||
                        targetBlock.State == BlockState.Landing))
                {
                    h_blocks.Add(targetBlock);
                }
            }

            sameBlocks.Add(block); //自己本身添加在内
            if (v_blocks.Count >= 2 && h_blocks.Count < 2)
            {
                sameBlocks.AddRange(v_blocks);
            }
            else if (v_blocks.Count < 2 && h_blocks.Count >= 2)
            {
                sameBlocks.AddRange(h_blocks);
            }
            else if (v_blocks.Count >= 2 && h_blocks.Count >= 2)
            {
                sameBlocks.AddRange(v_blocks);
                sameBlocks.AddRange(h_blocks);
            }

            return sameBlocks;
        }
        
    }
}
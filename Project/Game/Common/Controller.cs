using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Project;
using FrameWork.Manager;
using ResourceLoad;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Demo
{
    public class Controller
    {
        
        //自身所有棋子的初始化数据
        public Block[,] blockMatrix = new Block[ConstValues.MAX_MATRIX_ROW, ConstValues.MAX_COL];
        //自身待删除的blocks
        public List<Block> waitToDestoryBlocks = new List<Block>();
        //待添加的压力块数据(场外)
        public Dictionary<PressureType, Queue<string>> ToBeAddedPressBlocks = new Dictionary<PressureType, Queue<string>>()
        {
            {PressureType.Chain, new Queue<string>()},
            {PressureType.Combo, new Queue<string>()},
            {PressureType.Metal, new Queue<string>()}
        };
        
        //压力块列表(场内显示出的压力块)
        public List<PressureBlock> pressureBlocks = new List<PressureBlock>();
        //需要解锁的压力块列表
        public List<PressureBlock> unlockPressBlocks = new List<PressureBlock>();
        public List<Block> genBlocksByPressure = new List<Block>();
        //同一帧内的消除集合
        public List<Block> BlocksInSameFrame = new List<Block>();
        public string blockBufferWithNet;//多人模式下从服务器获取的block配置
        protected int genNewRowCount = 1; //构建新行的次数
        public Slider Hp_Slider;
        protected int Hp;

        public int HP
        {
            get { return Hp; }
            set { Hp = value; }
        }

        public int GenNewRowCount
        {
            get => genNewRowCount;
            set => genNewRowCount = value;
        }
        
        
        #region 初始化blockStageData部分
        /// <summary>
        /// 初始化创建所有blockData数据并返回
        /// </summary>
        /// <param name="stageConfigs">block排布配置</param>
        /// <param name="speedLevel">速度等级</param>
        /// <returns></returns>
        public List<BlockData> GenBlockDatas(List<string[]>stageConfigs,int speedLevel = 1)
        {
            int maxRow = ConstValues.MAX_GENROW, maxCol = ConstValues.MAX_COL;
            BlockData[,] blockDataMatrix = new BlockData[maxRow, maxCol];
            List<BlockData> blockDataList = new List<BlockData>();

            for (int row = 0; row < maxRow; row++)
            {
                for (int col = 0; col < maxCol; col++)
                {
                    BlockShape shape = BlockShape.None;

                    if (blockDataList.Count < 30)
                    {
                        do
                        {
                            shape = (BlockShape) Random.Range(1, 6); // Randomly generate a BlockType that is not None
                        } while ((speedLevel > 3 && IsSameAsAdjacent(blockDataMatrix, row, col, shape))
                                 || (row >= 2 && blockDataMatrix[row - 1, col].Shape == shape &&
                                     blockDataMatrix[row - 2, col].Shape == shape)
                                 || (col >= 2 && blockDataMatrix[row, col - 1].Shape == shape &&
                                     blockDataMatrix[row, col - 2].Shape == shape));
                    }
                    else if (row > 0 && blockDataMatrix[row - 1, col].Shape == BlockShape.None)
                    {
                        shape = BlockShape.None;
                    }

                    blockDataMatrix[row, col] = new BlockData(row + 1, col + 1, shape);
                    blockDataList.Add(blockDataMatrix[row, col]);
                }
            }

            if (Random.Range(0, 2) != 0)
            {
                blockDataList = MatchStageBlockDatas(blockDataList, stageConfigs[Random.Range(0, stageConfigs.Count)]);
            }

            return blockDataList;
        }
        
        /// <summary>
        /// 服务器数据初始化blocks
        /// </summary>
        /// <param name="blockBuffer"></param>
        /// <returns></returns>
        public List<BlockData> GenBlockDatasWith(string blockBuffer)
        {
            int maxRow = ConstValues.MAX_GENROW, maxCol = ConstValues.MAX_COL;
            List<BlockData> blockDataList = new List<BlockData>();
            string buffer = blockBuffer;

            int buffCount = 0;
            for (int row = 0; row < maxRow; row++)
            {
                for (int col = 0; col < maxCol; col++)
                {
                    if (buffCount < 42)
                    {
                        char numberStr = buffer[buffCount];
                        BlockShape shape = ConstValues.BLOCK_COLOR_NUMBER_TO_BLOCKSHAPE[numberStr];
                        blockDataList.Add(new BlockData(row + 1,col + 1,shape));
                        buffCount++;
                    }
                    else
                    {
                        blockDataList.Add(new BlockData(row + 1,col + 1,BlockShape.None));
                    }
                }
            }
            
            //在SelfGameController.Inst.blockBufferWithNet移除使用过的字符
            blockBufferWithNet = blockBufferWithNet.Remove(0, buffCount);
            Debug.LogError(blockBufferWithNet + "--" + blockBufferWithNet.Length);
            
            return blockDataList;
        }
        
        /// <summary>
        /// 服务器数据构建新一行blocks
        /// </summary>数据
        /// <param name="blockBuffer"></param>
        /// <returns></returns>
        public List<BlockData> GenRowBlockDatasWith(string blockBuffer)
        {
            List<BlockData> blockDataList = new List<BlockData>();
            for (int col = 0; col < ConstValues.MAX_COL; col++)
            {
                char numberStr = blockBuffer[col];
                BlockShape shape = ConstValues.BLOCK_COLOR_NUMBER_TO_BLOCKSHAPE[numberStr];
                blockDataList.Add(new BlockData(0, col + 1,shape));
            }
            blockBufferWithNet = blockBufferWithNet.Remove(0, ConstValues.MAX_COL);
            return blockDataList;
        }
        
        //判断block是否相邻且相同
        protected static bool IsSameAsAdjacent(BlockData[,] blockDataMatrix, int row, int col, BlockShape shape)
        {
            return (row > 0 && blockDataMatrix[row - 1, col].Shape == shape) ||
                   (col > 0 && blockDataMatrix[row, col - 1].Shape == shape);
        }
        
        //根据模板随机选择地图沟壑配置
        protected List<BlockData> MatchStageBlockDatas(List<BlockData> blockDatas, string[] stage)
        {
            List<int> startChangeIndexs = new List<int>(); //起始需要交换的位置
            List<int> endChangeIndexs = new List<int>(); //结束需要交换的位置
            for (int row = ConstValues.MAX_GENROW -5 - 1; row >= 0; row--)
            {
                for (int col = 0; col < ConstValues.MAX_COL; col++)
                {
                    int index = row * 6 + col;
                    int indexOfBlockDatas = (ConstValues.MAX_GENROW - 5 - 1 - row) * ConstValues.MAX_COL + col;

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
                BlockShape temp = blockData_1.Shape;
                blockData_1.Shape = blockData_2.Shape;
                blockData_2.Shape = temp;
            }

            return blockDatas;
        }
        
        //获取与之前不同的blockType
        public  BlockShape GetDiffTypeFrom(BlockShape oldShape)
        {
            BlockShape newShape = (BlockShape) Random.Range(1, ConstValues.MAX_BLOCKTYPE);
            while (oldShape == newShape)
            {
                newShape = (BlockShape) Random.Range(1, ConstValues.MAX_BLOCKTYPE);
            }

            return newShape;
        }
        #endregion
        
        #region 构建棋子部分
        /// <summary>
        /// 构建所有棋子
        /// </summary>
        /// <param name="datas"></param>
        /// <param name="boardTran"></param>
        public void GenBlocks(List<BlockData> datas, Transform boardTran,bool isSelf = true)
        {
            //遍历所有数据新建棋子
            for (int i = 0; i < datas.Count; i++)
            {
                var data = datas[i];
                int row = data.row;
                int col = data.col;
                BlockShape shape = data.Shape;

                // //空方块不生成对象
                // if(Type == BlockType.None)
                //     continue;

                GameObject prefabObj = ConstValues.BlockPrefabs[(int) shape];

                Block block =
                    Block.CreateBlockObject(prefabObj, row, col, false, shape, BlockState.Normal, boardTran, isSelf);
                //设置棋子位置
                block.transform.localPosition = new Vector3(
                     (isSelf ? ConstValues.SELF_BLOCK_X_ORIGINPOS : ConstValues.OTHER_BLOCK_X_ORIGINPOS) + (col - 1) * (isSelf ? ConstValues.SELF_BLOCK_X_OFFSET : ConstValues.OTHER_BLOCK_X_OFFSET),
                    (isSelf ? ConstValues.SELF_BLOCK_Y_ORIGINPOS : ConstValues.OTHER_BLOCK_Y_ORIGINPOS) + (row - 1) * (isSelf ? ConstValues.SELF_BLOCK_Y_OFFSET : ConstValues.OTHER_BLOCK_Y_OFFSET),
                    0f
                );
                // Debug.LogError(row + "-" + col);
                // Debug.LogError(row + "-" + col);
                blockMatrix[row, col - 1] = block;
                block.BlockOperationEvent += OnBlockOperation;
            }
        }
        
        /// <summary>
        /// 创建新的一行blocks--多人模式下
        /// </summary>
        /// <param name="genCount"></param>
        public void GenNewRowBlocksMultiplayer(int genCount = 1,bool isSelf = true)
        {
            BlockData[] newRowBlockData = GenRowBlockDatasWith(blockBufferWithNet).ToArray();
            GenNewRowBlocksByData(newRowBlockData, genCount, isSelf);
        }

        /// <summary>
        /// 创建新的一行的blockData
        /// </summary>
        /// <returns></returns>
        public BlockData[] GenNewRowDataSinglePlayer()
        {
            BlockData[] newRowBlockData = new BlockData[6];
            BlockShape oldShape = (BlockShape) Random.Range(1, 6);
            for (int i = 0; i < 6; i++)
            {
                oldShape = GetDiffTypeFrom(oldShape);
                newRowBlockData[i] = new BlockData(0, i + 1, oldShape);
            }

            return newRowBlockData;
        }
        
        /// <summary>
        /// 创建新的一行blocks--单人模式下
        /// </summary>
        public void GenNewRowBlocksSinglePlayer(BlockData[] newRowBlockData, int genCount = 1, bool isSelf = true)
        {
            GenNewRowBlocksByData(newRowBlockData, genCount, isSelf);
        }

        protected void GenNewRowBlocksByData(BlockData[] newRowBlockData,int genCount,bool isSelf)
        {
            List<Block> newRowBlocks = new List<Block>();
            var boardTran = isSelf
                ? UIManager.Inst.GetUI<GameView>(UIDef.GameView).Self_BlockBoard
                : UIManager.Inst.GetUI<GameView>(UIDef.GameView).Other_BlockBoard;

            //遍历生成新的block
            for (int i = 0; i < newRowBlockData.Length; i++)
            {
                var data = newRowBlockData[i];
                int row = data.row;
                int col = data.col;
                BlockShape shape = data.Shape;

                GameObject prefabObj = ConstValues.BlockPrefabs[(int) shape];
                Block block =
                    Block.CreateBlockObject(prefabObj, row, col, true, shape, BlockState.Dimmed, boardTran, isSelf);
                //设置棋子位置
                block.transform.localPosition = new Vector3(
                    (isSelf ? ConstValues.SELF_BLOCK_X_ORIGINPOS : ConstValues.OTHER_BLOCK_X_ORIGINPOS) + (col - 1) * ( isSelf ? ConstValues.SELF_BLOCK_X_OFFSET : ConstValues.OTHER_BLOCK_X_OFFSET),
                     (isSelf ? ConstValues.SELF_BLOCK_Y_ORIGINPOS : ConstValues.OTHER_BLOCK_Y_ORIGINPOS) + (row - genCount) * (isSelf ? ConstValues.SELF_BLOCK_Y_OFFSET : ConstValues.OTHER_BLOCK_Y_OFFSET),
                    0f
                );
                block.BlockOperationEvent += OnBlockOperation;
                newRowBlocks.Add(block);
            }
            
            if (genCount > 1)
            {
                //后面就先把原先每row的值上移
                for (int row = ConstValues.MAX_MATRIX_ROW - 1; row > 0; row--)
                {
                    for (int col = 0; col < ConstValues.MAX_COL; col++)
                    {
                        var targetBlock = blockMatrix[row, col];
                        targetBlock = blockMatrix[row - 1, col];
                        if (targetBlock)
                        {
                            targetBlock.Row = row;
                            targetBlock.Col = col + 1;
                            targetBlock.ChangeBlockObjName();
                            if (targetBlock.State == BlockState.Dimmed)
                                StateManger._instance.ChangeState(BlockState.Normal, targetBlock);
                            blockMatrix[row, col] = targetBlock;
                        }
                    }
                }
            }

            for (int i = 0; i < newRowBlocks.Count; i++)
            {
                blockMatrix[0, i] = newRowBlocks[i];
            }
            
            genNewRowCount++;
        }
        
        
        
        public Block GenNewBlock(int row, int col, BlockShape shape, bool genByGarbage,bool chain,bool isSelf = true)
        {
            Block block = blockMatrix[row, col - 1];
            block.State = BlockState.Normal;
            block.Shape = shape;
            block.GenByGarbage = genByGarbage;
            block.Chain = true;
            return block;
        }
        
        
        protected void OnBlockOperation(int row, int col, BlockOperation operation)
        {
            //执行拖拽操作
            if (operation == BlockOperation.DragHalf)
            {
                StateManger._instance.StateHandlers[BlockState.Swapping].OnBlockOperation(row, col, operation);
            }

            if (operation == BlockOperation.TouchUp || operation == BlockOperation.TouchDown)
            {
                StateManger._instance.StateHandlers[BlockState.Normal].OnBlockOperation(row, col, operation);
            }
        }
        #endregion
        
        #region 工具部分
        /// <summary>
        /// 根据类型加入压力块数据
        /// </summary>
        /// <param name="type"></param>
        /// <param name="count"></param>
        public void PushPressureDataWith(PressureType type, int count)
        {
            if (type == PressureType.Chain)
            {
                string config = $"{count - 1}_Rb";
                ToBeAddedPressBlocks[PressureType.Chain].Enqueue(config);
            }
            else if (type == PressureType.Combo)
            {
                string config = count <= 27
                    ? ConstValues.pressureConfWithCombo[count]
                    : ConstValues.pressureConfWithCombo[28];
                ToBeAddedPressBlocks[PressureType.Combo].Enqueue(config);
            }else if (type == PressureType.Metal)
            {
                string config = $"{count - 2}*Rb";
                ToBeAddedPressBlocks[PressureType.Metal].Enqueue(config);
            }
        }
        
        //按顺序弹出压力块数据
        public string PopPressureDataWith()
        {
            string result = null;
            if (ToBeAddedPressBlocks[PressureType.Chain].Count > 0)
            {
                result = ToBeAddedPressBlocks[PressureType.Chain].Dequeue();
            }
            else if (ToBeAddedPressBlocks[PressureType.Combo].Count > 0)
            {
                result = ToBeAddedPressBlocks[PressureType.Combo].Dequeue();
            }
            else if (ToBeAddedPressBlocks[PressureType.Metal].Count > 0)
            {
                result = ToBeAddedPressBlocks[PressureType.Metal].Dequeue();
            }
            
            return result;
        }

        public void ClearPressureData()
        {
            foreach (var type in ToBeAddedPressBlocks.Keys)
            {
                ToBeAddedPressBlocks[type].Clear();
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
                if (targetBlock && targetBlock.Shape == BlockShape.None || targetBlock.GenByGarbage)
                    break;
                
                if ((curBlock.Shape == targetBlock.Shape)
                    && (curBlock.Shape == block.Shape)
                    && (targetBlock.State == BlockState.Normal ||
                        targetBlock.State == BlockState.Landing))
                {
                    v_blocks.Add(targetBlock);
                }
                else
                {
                    break;
                }
            }

            //向下找
            for (int row = curRow; row > 1; row--)
            {
                var targetBlock = blockMatrix[row - 1, curCol];
                var curBlock = blockMatrix[row, curCol];
                if (targetBlock.Shape == BlockShape.None || targetBlock.GenByGarbage)
                    break;
                if ((curBlock.Shape == targetBlock.Shape)
                    && (curBlock.Shape == block.Shape)
                    && (targetBlock.State == BlockState.Normal ||
                        targetBlock.State == BlockState.Landing))
                {
                    v_blocks.Add(targetBlock);
                }
                else
                {
                    break;
                }
            }

            //向左找
            for (int col = curCol; col > 0; col--)
            {
                var targetBlock = blockMatrix[curRow, col - 1];
                var curBlock = blockMatrix[curRow, col];
                if (targetBlock.Shape == BlockShape.None || targetBlock.GenByGarbage)
                    break;
                if ((curBlock.Shape == targetBlock.Shape)
                    && (curBlock.Shape == block.Shape)
                    && (targetBlock.State == BlockState.Normal ||
                        targetBlock.State == BlockState.Landing))
                {
                    h_blocks.Add(targetBlock);
                }
                else
                {
                    break;
                }
            }

            //向右找
            for (int col = curCol; col < ConstValues.MAX_COL - 1; col++)
            {
                var targetBlock = blockMatrix[curRow, col + 1];
                var curBlock = blockMatrix[curRow, col];
                if (targetBlock.Shape == BlockShape.None || targetBlock.GenByGarbage)
                    break;
                if ((curBlock.Shape == targetBlock.Shape)
                    && (curBlock.Shape == block.Shape)
                    && (targetBlock.State == BlockState.Normal ||
                        targetBlock.State == BlockState.Landing))
                {
                    h_blocks.Add(targetBlock);
                }
                else
                {
                    break;
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

            //给获取的sameblocks排序，从上到下，从左到右，方便后面的按顺序播放效果
            //left-right
            sameBlocks.Sort((block1, block2) => block1.Col.CompareTo(block2.Col));
            //up-down
            sameBlocks.Sort((block1, block2) => block2.Row.CompareTo(block1.Row));

            return sameBlocks;
        }
        
        /// <summary>
        /// 设置当前block上方同列所有非None的block的chain为True
        /// </summary>
        /// <param name="block"></param>
        public void SetUpRowBlockChain(Block block)
        {
            int beginRow = block.Row;
            int beginCol = block.Col - 1;
            for (int row = beginRow + 1; row <= ConstValues.MAX_ROW; row++)
            {
                var targetBlock = blockMatrix[row, beginCol];
                if(targetBlock.Shape == BlockShape.None || IsBlockInSameFrame(targetBlock))
                    break;
                targetBlock.Chain = true;
                //Debug.LogError($"{targetBlock.name}-chain-{targetBlock.Chain}");
            }
        }
        
        // 判断要找额block是否在同一帧要消除的棋子集合中
        public bool IsBlockInSameFrame(Block targetBlock)
        {
            return BlocksInSameFrame.Contains(targetBlock);
        }
        
        /// <summary>
        /// 构建并显示combo物体
        /// </summary>
        /// <param name="num"></param>
        /// <param name="localPos"></param>
        public void GenComboObj(int num, Vector3 localPos,bool isSelf = true)
        {
            SingletonManager.GetManager<ResourcesManager>().LoadPrefab(
                ConstValues.comboPrefabPath,
                (originObj, l) =>
                {
                    var gameView = UIManager.Inst.GetUI<GameView>(UIDef.GameView);
                    Transform effectArea = isSelf ? gameView.Self_EffectArea : gameView.Other_EffectArea;
                    GameObject obj = GameObject.Instantiate(originObj, effectArea);
                    obj.transform.localPosition = new Vector3(localPos.x - (isSelf ? ConstValues.SELF_BLOCK_WIDTH : ConstValues.OTHER_BLOCK_WIDTH) / 2f,
                        localPos.y + (isSelf ? ConstValues.SELF_BLOCK_WIDTH : ConstValues.OTHER_BLOCK_WIDTH ) / 2f, 0f);
                    obj.transform.localScale = isSelf ? Vector3.one : new Vector3(0.5f, 0.5f, 0.5f);
                    Combo comb = obj.gameObject.GetComponent<Combo>();
                    comb.Show(num);
                });
        }
        
        /// <summary>
        /// 构建并显示chain物体
        /// </summary>
        /// <param name="num"></param>
        /// <param name="localPos"></param>
        public void GenChainObj(int num, Vector3 localPos,bool isSelf = true)
        {
            SingletonManager.GetManager<ResourcesManager>().LoadPrefab(
                ConstValues.chainPrefabPath,
                (originObj, l) =>
                {
                    Transform effectArea = UIManager.Inst.GetUI<GameView>(UIDef.GameView).Self_EffectArea;
                    GameObject obj = GameObject.Instantiate(originObj, effectArea);
                    obj.transform.localPosition = new Vector3(localPos.x - (isSelf ? ConstValues.SELF_BLOCK_WIDTH : ConstValues.OTHER_BLOCK_WIDTH) / 2f,
                        localPos.y + (isSelf ? ConstValues.SELF_BLOCK_WIDTH : ConstValues.OTHER_BLOCK_WIDTH) / 2f, 0f);
                    Chain chainObj = obj.gameObject.GetComponent<Chain>();
                    chainObj.Show(num);
                });
        }
        
        
        private void OutputBlockDataMatrix()
        {
            StringBuilder stb = new StringBuilder();
            for (int row = ConstValues.MAX_MATRIX_ROW - 1; row >= 0; row--)
            {
                for (int col = 0; col < ConstValues.MAX_COL; col++)
                {
                    if (blockMatrix[row, col])
                    {
                        stb.Append(blockMatrix[row, col].name);
                        stb.Append(",\t");
                    }
                    else
                    {
                        stb.Append("NULL");
                        stb.Append(",\t");
                    }
                }

                stb.Append("\n");
            }

            Debug.LogError(stb.ToString());
        }
        
        /// <summary>
        /// 获取block是否在和压力块重合
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public bool CheckPressureBlockIncludeBlock(Block block)
        {
            bool result = false;
            for (int i = 0; i < pressureBlocks.Count; i++)
            {
                if(result)
                    break;
                
                var pressureBlock = pressureBlocks[i];
                result = block.PressureBlockIncludeBlock(pressureBlock);
            }

            return result;
        }
        
        /// <summary>
        /// 清空删除所有Blocks和压力块
        /// </summary>
        public void ClearAllBlockObj()
        {
            //清空blocks
            for (int i = 0; i < blockMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < blockMatrix.GetLength(1); j++)
                {
                    if (blockMatrix[i, j] != null)
                    {
                        GameObject.Destroy(blockMatrix[i,j].gameObject);
                        blockMatrix[i, j] = null;
                    }
                }
            }
            
            //清空压力块
            for (int i = 0; i < pressureBlocks.Count; i++)
            {
                var pressureBlockObj = pressureBlocks[i].gameObject;
                GameObject.Destroy(pressureBlockObj);
            }
            
            pressureBlocks.Clear();
            unlockPressBlocks.Clear();
            BlocksInSameFrame.Clear();
        }
        
        /// <summary>
        /// 检测block是否在集合中
        /// </summary>
        /// <param name="targetBlock"></param>
        /// <returns></returns>
        public bool CheckBlcokInBlocks(Block targetBlock)
        {
            bool result = false;
            foreach (var block in blockMatrix)
            {
                if (block == targetBlock)
                {
                    result = true;
                    break;
                }
            }
            return result;
        }
        
        /// <summary>
        /// 获取blocks中最高层数
        /// </summary>
        /// <returns></returns>
        public int GetMaxRowOfBlocks()
        {
            int maxRow = 1;
            foreach (var block in blockMatrix)
            {
                if (block)
                {
                    if(block.Shape != BlockShape.None)
                        maxRow = block.Row > maxRow ? block.Row : maxRow;
                }
                    
            }

            return maxRow;
        }
        
        //获取压力快总层数
        public int GetTotalRowOfPressure()
        {
            int totalRows = 0;
            foreach (var pressure_b in pressureBlocks)
            {
                if(pressure_b.State == BlockState.Normal && pressure_b.Row > GetMaxRowOfBlocks())
                    totalRows += pressure_b.Y_Height;
            }

            return totalRows;
        }
        
        #endregion
    }
}
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Project;
using FrameWork.Manager;
using ResourceLoad;
using Random = UnityEngine.Random;

namespace Demo
{
    public class Controller
    {
        
        //自身所有棋子的初始化数据
        public Block[,] blockMatrix = new Block[ConstValues.MAX_MATRIX_ROW, ConstValues.MAX_COL];
        //压力块列表(场上显示出的压力块)
        public List<PressureBlock> pressureBlocks = new List<PressureBlock>();
        //需要解锁的压力块列表
        public List<PressureBlock> unlockPressBlocks = new List<PressureBlock>();
        //同一帧内的消除集合
        public List<List<Block>> BlocksInSameFrame = new List<List<Block>>();
        
        protected int genNewRowCount = 1; //构建新行的次数

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
        public void GenBlocks(List<BlockData> datas, Transform boardTran)
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
                    Block.CreateBlockObject(prefabObj, row, col, false, shape, BlockState.Normal, boardTran, this);
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
        }
        
        /// <summary>
        /// 创建新的一行blocks
        /// </summary>
        public void GenNewRowBlocks(int genCount = 1)
        {
            List<Block> newRowBlocks = new List<Block>();
            BlockData[] newRowBlockData = new BlockData[6];
            BlockShape oldShape = (BlockShape) Random.Range(1, 6);
            for (int i = 0; i < 6; i++)
            {
                oldShape = GetDiffTypeFrom(oldShape);
                newRowBlockData[i] = new BlockData(0, i + 1, oldShape);
            }
            
            var boardTran = UIManager.Inst.GetUI<GameView>(UIDef.GameView).BlockBoard;

            //遍历生成新的block
            for (int i = 0; i < newRowBlockData.Length; i++)
            {
                var data = newRowBlockData[i];
                int row = data.row;
                int col = data.col;
                BlockShape shape = data.Shape;

                GameObject prefabObj = ConstValues.BlockPrefabs[(int) shape];
                Block block =
                    Block.CreateBlockObject(prefabObj, row, col, true, shape, BlockState.Dimmed, boardTran, this);
                //设置棋子位置
                block.transform.localPosition = new Vector3(
                    ConstValues.BLOCK_X_ORIGINPOS + (col - 1) * ConstValues.BLOCK_X_OFFSET,
                    ConstValues.BLOCK_Y_ORIGINPOS + (row - genCount) * ConstValues.BLOCK_Y_OFFSET,
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
        }
        
        public Block GenNewBlock(int row, int col, BlockShape shape, bool genByGarbage,bool chain)
        {
            Block block = null;
            if (blockMatrix[row, col - 1] != null)
            {
                block = blockMatrix[row, col - 1];
                // GameObject.Destroy(blockMatrix[row, col - 1].gameObject);
                // blockMatrix[row, col - 1] = null;
                block.State = BlockState.Normal;
                block.Shape = shape;
                block.GenByGarbage = genByGarbage;
                block.Chain = true;
            }
            else
            {
                
                var boardTran = UIManager.Inst.GetUI<GameView>(UIDef.GameView).BlockBoard;
                GameObject prefabObj = ConstValues.BlockPrefabs[(int) shape];

                block = Block.CreateBlockObject(prefabObj, row, col, false, shape, BlockState.Normal, boardTran, this);
                //设置棋子位置
                block.transform.localPosition = new Vector3(
                    ConstValues.BLOCK_X_ORIGINPOS + (col - 1) * ConstValues.BLOCK_X_OFFSET,
                    ConstValues.BLOCK_Y_ORIGINPOS + (row - genNewRowCount + 1) * ConstValues.BLOCK_Y_OFFSET,
                    0f
                );
                block.GenByGarbage = genByGarbage;
                block.Chain = chain;
                
                blockMatrix[row, col - 1] = block;
                block.BlockOperationEvent += OnBlockOperation;
            }
            
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
                if ((targetBlock && targetBlock.Shape == BlockShape.None) || !targetBlock)
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
                if (targetBlock.Shape == BlockShape.None)
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
                if (targetBlock.Shape == BlockShape.None)
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
                if (targetBlock.Shape == BlockShape.None)
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
                Debug.LogError($"{targetBlock.name}-chain-{targetBlock.Chain}");
            }
        }
        
        // 判断要找额block是否在同一帧要消除的棋子集合中
        private bool IsBlockInSameFrame(Block targetBlock)
        {
            bool result = false;
            foreach (var sameBlocks in BlocksInSameFrame)
            {
                if (sameBlocks.Contains(targetBlock))
                {
                    result = true;
                    break;
                }
            }

            return result;
        }
        
        /// <summary>
        /// 构建并显示combo物体
        /// </summary>
        /// <param name="num"></param>
        /// <param name="localPos"></param>
        public void GenComboObj(int num, Vector3 localPos)
        {
            SingletonManager.GetManager<ResourcesManager>().LoadPrefab(
                ConstValues.comboPrefabPath,
                (originObj, l) =>
                {
                    Transform effectArea = UIManager.Inst.GetUI<GameView>(UIDef.GameView).EffectArea;
                    GameObject obj = GameObject.Instantiate(originObj, effectArea);
                    obj.transform.localPosition = new Vector3(localPos.x - ConstValues.BLOCK_WIDTH / 2f,
                        localPos.y + ConstValues.BLOCK_WIDTH / 2f, 0f);
                    Combo comb = obj.gameObject.GetComponent<Combo>();
                    comb.Show(num);
                });
        }
        
        /// <summary>
        /// 构建并显示chain物体
        /// </summary>
        /// <param name="num"></param>
        /// <param name="localPos"></param>
        public void GenChainObj(int num, Vector3 localPos)
        {
            SingletonManager.GetManager<ResourcesManager>().LoadPrefab(
                ConstValues.chainPrefabPath,
                (originObj, l) =>
                {
                    Transform effectArea = UIManager.Inst.GetUI<GameView>(UIDef.GameView).EffectArea;
                    GameObject obj = GameObject.Instantiate(originObj, effectArea);
                    obj.transform.localPosition = new Vector3(localPos.x - ConstValues.BLOCK_WIDTH / 2f,
                        localPos.y + ConstValues.BLOCK_WIDTH / 2f, 0f);
                    Chain comb = obj.gameObject.GetComponent<Chain>();
                    Debug.LogError(comb);
                    comb.Show(num);
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
        
        #endregion
    }
}
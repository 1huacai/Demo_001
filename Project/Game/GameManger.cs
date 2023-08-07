using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Project;
using FrameWork.Manager;
using ResourceLoad;
using Random = UnityEngine.Random;
using Spine.Unity;

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
        public bool gameStart = false; //游戏开始标志


        public GameManger()
        {
        }


        //自身所有棋子的初始化数据
        public Block[,] blockMatrix = new Block[ConstValues.MAX_MATRIX_ROW, ConstValues.MAX_COL];
        //压力块列表(场上显示出的压力块)
        public List<PressureBlock> pressureBlocks = new List<PressureBlock>();
        public List<PressureBlock> unlockPressBlocks = new List<PressureBlock>();

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
                blockDataList = MatchStageBlockDatas(blockDataList, stageConfigs[Random.Range(0, stageConfigs.Count)]);
            }

            return blockDataList;
        }

        private bool IsSameAsAdjacent(BlockData[,] blockDataMatrix, int row, int col, BlockType Type)
        {
            return (row > 0 && blockDataMatrix[row - 1, col].type == Type) ||
                   (col > 0 && blockDataMatrix[row, col - 1].type == Type);
        }

        //根据模板随机选择地图沟壑配置
        private List<BlockData> MatchStageBlockDatas(List<BlockData> blockDatas, string[] stage)
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
                BlockType temp = blockData_1.type;
                blockData_1.type = blockData_2.type;
                blockData_2.type = temp;
            }

            return blockDatas;
        }

        //获取与之前不同的blockType
        public BlockType GetDiffTypeFrom(BlockType oldType)
        {
            BlockType newType = (BlockType) Random.Range(1, ConstValues.MAX_BLOCKTYPE);
            while (oldType == newType)
            {
                newType = (BlockType) Random.Range(1, ConstValues.MAX_BLOCKTYPE);
            }

            return newType;
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
            SingletonManager.GetManager<ResourcesManager>().LoadPrefab(ConstValues.blockPrefabPath, (obj, l) =>
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

                    Block block =
                        Block.CreateBlockObject(obj, row, col, false, Type, BlockState.Normal, boardTran, this);
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


        private List<Block> newRowBlocks = new List<Block>();

        /// <summary>
        /// 创建新的一行blocks
        /// </summary>
        public void GenNewRowBlocks(int genCount = 1)
        {
            BlockData[] newRowBlockData = new BlockData[6];
            BlockType oldType = (BlockType) Random.Range(1, 6);
            for (int i = 0; i < 6; i++)
            {
                oldType = GetDiffTypeFrom(oldType);
                newRowBlockData[i] = new BlockData(0, i + 1, oldType);
            }

            newRowBlocks.Clear();
            var boardTran = UIManager.Inst.GetUI<GameView>(UIDef.GameView).BlockBoard;
            SingletonManager.GetManager<ResourcesManager>().LoadPrefab(ConstValues.blockPrefabPath, ((obj, length) =>
            {
                //遍历生成新的block
                for (int i = 0; i < newRowBlockData.Length; i++)
                {
                    var data = newRowBlockData[i];
                    int row = data.row;
                    int col = data.col;
                    BlockType Type = data.type;

                    Block block =
                        Block.CreateBlockObject(obj, row, col, true, Type, BlockState.Dimmed, boardTran, this);
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
                                if(targetBlock.State == BlockState.Dimmed)
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
            }));
        }

        public Block GenNewBlock(int row,int col,BlockType type,bool genByGarbage)
        {
            Block block = null;
            var boardTran = UIManager.Inst.GetUI<GameView>(UIDef.GameView).BlockBoard;
            SingletonManager.GetManager<ResourcesManager>().LoadPrefab(ConstValues.blockPrefabPath, ((obj, length) =>
            {
                block = Block.CreateBlockObject(obj, row, col, false, type, BlockState.Normal, boardTran, this);
                //设置棋子位置
                block.transform.localPosition = new Vector3(
                    ConstValues.BLOCK_X_ORIGINPOS + (col - 1) * ConstValues.BLOCK_X_OFFSET,
                    ConstValues.BLOCK_Y_ORIGINPOS + (row - genNewRowCount + 1) * ConstValues.BLOCK_Y_OFFSET,
                    0f
                );
                block.GenByGarbage = genByGarbage;
                if (blockMatrix[row, col - 1] != null)
                {
                    GameObject.Destroy(blockMatrix[row, col - 1].gameObject);
                    blockMatrix[row, col - 1] = null;
                }
                blockMatrix[row, col - 1] = block;
                block.BlockOperationEvent += OnBlockOperation;
            }));
            return block;
        }
        
        private void OnBlockOperation(int row, int col, BlockOperation operation)
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
        
        
        public Transform boards = null;
        private Transform blockBoard = null;
        private Transform pressureBoard = null;
        private int genNewRowCount = 1; //构建新行的次数

        public int GenNewRowCount
        {
            get => genNewRowCount;
            set => genNewRowCount = value;
        }

        #region 游戏逻辑部分
        
        //初始化游戏
        public void InitGame()
        {  
            Application.targetFrameRate = ConstValues.targetPlatformFps;
            var gameView = UIManager.Inst.GetUI<GameView>(UIDef.GameView);
            var blockDatas = GameManger.Inst.GenBlockDatas();
            boards = UIManager.Inst.GetUI<GameView>(UIDef.GameView).Boards;
            blockBoard = UIManager.Inst.GetUI<GameView>(UIDef.GameView).BlockBoard;
            pressureBoard = UIManager.Inst.GetUI<GameView>(UIDef.GameView).PressureBoard;
            
            //根据数据构建所有棋子obj
            GenBlocks(blockDatas, gameView.BlockBoard);
            StateManger._instance.Init(this);
            TimerMgr._Instance.Init();
            gameStart = true;
        }

        public void FiexdUpdate()
        {
            if (!gameStart)
            {
                genNewRowCount = 1;
                return;
            }
            UpDateBlockArea();
        }

        public void LateUpdate()
        {
            if (!gameStart)
                return;
            LateUpdateBlockArea();
        }

        private bool boardStopRise = false;

        public bool BoardStopRise
        {
            get { return boardStopRise; }
            set { boardStopRise = value; }
        }

        private bool preussUnlocking = false;// 一堆压力块正在解锁的标志

        public bool PreussUnlocking
        {
            get { return preussUnlocking; }
            set { preussUnlocking = value; }
        }
        
        //更新棋盘区域逻辑
        private void UpDateBlockArea()
        {
            if(!BoardStopRise&&!PreussUnlocking)
                BoardRise();
            
            //检测每个block的自有逻辑
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
            
            //检测每个压力块的自有逻辑
            for (int i = 0; i < pressureBlocks.Count; i++)
            {
                pressureBlocks[i].LogicUpdate();
            }
        }


        public List<List<Block>> BlocksInSameFrame = new List<List<Block>>();

        private int count = 0;
        private void LateUpdateBlockArea()
        {
            
            if (BlocksInSameFrame.Count > 0)
            {
                Debug.LogError("同帧率多消组合FPS-" + TimerMgr._Instance.Frame);
                count = 0;
                
                for (int i = 0; i < BlocksInSameFrame.Count; i++)
                {
                    for (int j = 0; j < BlocksInSameFrame[i].Count; j++)
                    {
                        count++;
                        var block = BlocksInSameFrame[i][j];
                        
                        for (int k = 0; k < pressureBlocks.Count; k++)
                        {
                            var pressureBlock = pressureBlocks[k];
                            pressureBlock.UnlockPressureBlock(block.Row,block.Col);
                        }
                    }
                }
                
                if (count >= 4)//原数字是4，暂时换3测试
                {
                    GenComboObj(count, BlocksInSameFrame[0][0].transform.localPosition);
                    
                    //Combo达成，棋盘暂停移动
                    BoardStopRise = true;
                    TimerMgr._Instance.Schedule(() =>
                    {
                        BoardStopRise = false;
                        //兴建压力块
                        PressureBlock.CreatePressureBlock(true,count,pressureBoard);

                    }, 60 * ConstValues.fpsTime);
                }
                BlocksInSameFrame.Clear();
            }
        }
        
        //棋盘上升
        private void BoardRise()
        {
            //TODO 到达顶部，就不上升了
            
            if (TimerMgr._Instance.Frame % ConstValues.Rise_Times[7] == 0)
            {
                if (boards.transform.localPosition.y % ConstValues.BLOCK_Y_OFFSET == 0)
                {
                    GenNewRowBlocks(genNewRowCount);
                    genNewRowCount++;
                    //压力块的Row也更新+1
                    for (int i = 0; i < pressureBlocks.Count; i++)
                    {
                        pressureBlocks[i].Row++;
                    }
                }

                boards.transform.localPosition += new Vector3(0, 1, 0);
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
                if ((targetBlock && targetBlock.Type == BlockType.None) || !targetBlock)
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

            //给获取的sameblocks排序，从上到下，从左到右，方便后面的按顺序播放效果
            //left-right
            sameBlocks.Sort((block1, block2) => block1.Col.CompareTo(block2.Col));
            //up-down
            sameBlocks.Sort((block1, block2) => block2.Row.CompareTo(block1.Row));

            return sameBlocks;
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
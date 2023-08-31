using System.Collections.Generic;
using UnityEngine;
using Project;


namespace Demo
{
    public class SelfGameController : Controller
    {
        private static SelfGameController s_inst;

        public static SelfGameController Inst
        {
            get
            {
                if (s_inst == null)
                {
                    s_inst = new SelfGameController();
                }

                return s_inst;
            }
        }

        public Block selectBlock;
        public bool gameStart = false; //游戏开始标志

        //所有stage的样式的配置
        public List<string[]> stageConfigs = new List<string[]>
        {
            ConstValues.stage_1, ConstValues.stage_2
        };

        public Transform boards;
        public Transform blockBoard;
        public Transform pressureBoard;
        public float blockBoardOffsetX;
        private BlockData[] newRowBlockDatas;

        //比赛中玩家和对手用户名
        public string selfUserName;
        public string otehrUserName;
        
        private bool boardStopRise = false;

        public bool BoardStopRise 
        {
            get { return boardStopRise; }
            set { boardStopRise = value; }
        }

        private bool preussUnlocking = false; // 一堆压力块正在解锁的标志
        
        public bool PreussUnlocking
        {
            get { return preussUnlocking; }
            set { preussUnlocking = value; }
        }

        private bool _riseUpBtn = false;

        public bool riseUpBtn
        {
            get { return _riseUpBtn; }
            set { _riseUpBtn = value; }
        }
        
        //棋盘进入最高行，顶格状态
        private bool enterTopRow = false;

        public bool EnterTopRow
        {
            get { return enterTopRow; }
            set { enterTopRow = value; }
        }
        
        #region 游戏逻辑部分

        //初始化游戏
        public void InitGame()
        {
            Application.targetFrameRate = ConstValues.targetPlatformFps;
            boards = UIManager.Inst.GetUI<GameView>(UIDef.GameView).Self_Board;
            blockBoard = UIManager.Inst.GetUI<GameView>(UIDef.GameView).Self_BlockBoard;
            pressureBoard = UIManager.Inst.GetUI<GameView>(UIDef.GameView).Self_PressureBoard;
            blockBoardOffsetX = UIManager.Inst.GetUI<GameView>(UIDef.GameView).SelfBlockBoardOffsetX;
            Hp_Slider = UIManager.Inst.GetUI<GameView>(UIDef.GameView).Self_Hp_Slider;
            Hp_Slider.maxValue = ConstValues.Max_Player_Hp;
            Hp_Slider.value = ConstValues.Max_Player_Hp;
            HP = ConstValues.Max_Player_Hp;
            
            boards.localPosition = Vector3.zero;
            
            chainCount = 1;
            genNewRowCount = 1;
            pressureBlocks.Clear();
            ClearPressureData();
        }
        
        public void FiexdUpdate()
        {
            if (!gameStart)
            {
                //genNewRowCount = 1;
                return;
            }

            //更新棋盘状态
            UpdateBoardState();
            
            UpdateBlockArea();
            //自动清除待删除的block
            UpdateWaitToDestoryBlocks();
            
            
            if(NetManager.Instance.Multiplayer)
                NetManager.Instance.UpdateWebLogs(blockMatrix);
        }

        public void LateUpdate()
        {
            if (!gameStart)
                return;
            LateUpdateBlockArea();
        }

        //更新棋盘区域逻辑
        private void UpdateBlockArea()
        {
            if (!BoardStopRise && !PreussUnlocking && !EnterTopRow)
            {
                BoardRise(riseUpBtn);
                //从数据中弹出压力块
                var selfConfig = PopPressureDataWith();
                PressureBlock.CreatePressureBlock(selfConfig,pressureBoard,true);
                
                // if (!NetManager.Instance.Multiplayer)
                // {
                //     var otherConfig = OtherGameController.Inst.PopPressureDataWith();
                //     PressureBlock.CreatePressureBlock(otherConfig,OtherGameController.Inst.pressureBoard,false);
                //     OtherGameController.Inst.BoardRise(newRowBlockDatas, riseUpBtn);
                // }
                riseUpBtn = false;
            }
            
            //检测每个block的自有逻辑
            foreach (var block in blockMatrix)
            {
                if (block != null)
                {
                    block.LogicUpdate();
                }
            }
            
            //检测每个压力块的自有逻辑
            for (int i = 0; i < pressureBlocks.Count; i++)
            {
                pressureBlocks[i].LogicUpdate();
            }
            
            // //TODO 暂时放在这里执行对手压力块的逻辑
            // for (int i = 0; i < OtherGameController.Inst.pressureBlocks.Count; i++)
            // {
            //     OtherGameController.Inst.pressureBlocks[i].LogicUpdate();
            // }
        }
        
        /// <summary>
        /// 更新待删除的blocks
        /// </summary>
        private void UpdateWaitToDestoryBlocks()
        {
            waitToDestoryBlocks.Clear();
            for (int i = 0; i < blockBoard.childCount; i++)
            {
                var block = blockBoard.GetChild(i).GetComponent<Block>();
                if(!CheckBlcokInBlocks(block))
                    waitToDestoryBlocks.Add(block);//不在集合中，那么就加入待删除集合
            }
            
            //删除待消除的集合
            foreach (var destoryBlock in waitToDestoryBlocks)
            {
                GameObject.Destroy(destoryBlock.gameObject);
            }
        }

        private int curHeight = 0;
        /// <summary>
        /// 更新棋盘状态
        /// </summary>
        private void UpdateBoardState()
        {
            curHeight = GetMaxRowOfBlocks() + GetTotalRowOfPressure();
            if (curHeight == ConstValues.MAX_ROW - 1)
            {
                //TODO 危险状态，处于危险状态的的同列棋子开始闪烁
                Debug.LogError("进入危险状态");
            }

            if (curHeight >= ConstValues.MAX_ROW)
            {
                Debug.LogError("进入顶格状态");
                EnterTopRow = true;
                //己方扣血
                //如果处于压力快解锁状态就不扣血
                if (!preussUnlocking)
                {
                    Hp--;
                    Hp_Slider.value = Hp;
                    if (Hp <= 0)
                    {
                        gameStart = false;
                    }
                }
            }
            else
            {
                EnterTopRow = false;
            }
            
        }
        
        private int comboCount = 0;
        private int chainCount = 1;

        private void LateUpdateBlockArea()
        {
            if (BlocksInSameFrame.Count >= 3)
            {
                comboCount = 0;
                
                for (int i = 0; i < BlocksInSameFrame.Count; i++)
                {
                    var targetBlock = BlocksInSameFrame[i];
                    StateManger._instance.ChangeState(BlockState.Matched, targetBlock);
                    //设置该棋子上方的棋子chain为true
                    SetUpRowBlockChain(targetBlock);
                }

                Dictionary<BlockShape, List<Block>> tempDict = new Dictionary<BlockShape, List<Block>>();
                foreach (var block in BlocksInSameFrame)
                {
                    if (!tempDict.ContainsKey(block.Shape))
                    {
                        tempDict.Add(block.Shape,new List<Block>(){block});
                    }
                    else
                    {
                        tempDict[block.Shape].Add(block);
                    }
                }

                foreach (var key in tempDict.Keys)
                {
                    var blockList = tempDict[key];
                    if (blockList.Find((block => block.Chain)) != null)
                    {
                        chainCount++;
                    }
                }
                
                //集合中的chain数量
                // chainCount += BlocksInSameFrame.FindAll((block => block.Chain)).Count;
                comboCount = BlocksInSameFrame.Count;
             
                if (NetManager.Instance.Multiplayer)
                {
                    NetManager.Instance.GameMatched(TimerMgr._Instance.Frame,BlocksInSameFrame, 0,chainCount,null);
                }
                
                
                foreach (var block in BlocksInSameFrame)
                {
                    //解锁压力块
                    for (int k = 0; k < pressureBlocks.Count; k++)
                    {
                        //单人镜像镜像敌方解锁压力块
                        if (NetManager.Instance.Multiplayer)
                        {
                            // var pressureBlock_other = OtherGameController.Inst.pressureBlocks[k];
                            // pressureBlock_other.UnlockPressureBlock(block.Row, block.Col);
                        }
                        else
                        {
                            var pressureBlock_self = pressureBlocks[k];
                            pressureBlock_self.UnlockPressureBlock(block.Row, block.Col);
                        }
                    }
                }
                
                // combo压力块
                if (comboCount >= 4) 
                {
                    var targetBlock_self = BlocksInSameFrame[0];
                    GenComboObj(comboCount, targetBlock_self.transform.localPosition,true);
                    //给对手添加压力块数据
                    OtherGameController.Inst.PushPressureDataWith(PressureType.Combo,comboCount);
                    
                    if (!NetManager.Instance.Multiplayer)
                    {
                        var otherController = OtherGameController.Inst;
                        var targetBlock_other = otherController.blockMatrix[targetBlock_self.Row, targetBlock_self.Col - 1];
                        otherController.GenComboObj(comboCount,targetBlock_other.transform.localPosition,false);
                        //对手给玩家添加压力块数据
                        SelfGameController.Inst.PushPressureDataWith(PressureType.Combo,comboCount);
                    }
                    
                    //Combo达成，棋盘暂停移动
                    BoardStopRise = true;
                    TimerMgr._Instance.Schedule(() =>
                    {
                        BoardStopRise = false;
                    }, (20 * comboCount - 20) * ConstValues.fpsTime);
                }

                //chain压力块
                if (chainCount >= 2)
                {
                    var targetBlock_self = BlocksInSameFrame[0];
                    GenChainObj(chainCount, targetBlock_self.transform.localPosition);
                    //给对手添加压力块数据
                    //OtherGameController.Inst.PushPressureDataWith(PressureType.Chain,chainCount);
                    
                    if (!NetManager.Instance.Multiplayer)
                    {
                        // var otherController = OtherGameController.Inst;
                        // var targetBlock_other = otherController.blockMatrix[targetBlock_self.Row, targetBlock_self.Col - 1];
                        // otherController.GenChainObj(chainCount,targetBlock_other.transform.localPosition,false);
                        //对手给玩家添加压力块数据
                        SelfGameController.Inst.PushPressureDataWith(PressureType.Chain,chainCount);
                    }
                    
                    //chain达成
                    BoardStopRise = true;
                    TimerMgr._Instance.Schedule(() =>
                    {
                        BoardStopRise = false;
                        ChainEnd();
                    }, (20 * chainCount + 80) * ConstValues.fpsTime);
                }
                BlocksInSameFrame.Clear();
            }
        }
        
        int gameRiseType = 2;//默认是自动提升，多人模式下提升棋盘的操作类型
        private void BoardRise(bool btnRise = false)
        {
            //TODO 到达顶部，就不上升了
            if (btnRise)
            {
                int index = (int)(boards.transform.localPosition.y / ConstValues.SELF_BLOCK_Y_OFFSET);
                boards.transform.localPosition = new Vector3(0, (index + 1) * ConstValues.SELF_BLOCK_Y_OFFSET, 0);
                gameRiseType = 1;
            }
            else
            {
                if (TimerMgr._Instance.Frame % ConstValues.Rise_Times[7] == 0)
                {
                    if (boards.transform.localPosition.y %    ConstValues.SELF_BLOCK_Y_OFFSET == 0)
                    {
                        if (NetManager.Instance.Multiplayer)
                        {
                            NetManager.Instance.GameRaiseReq(TimerMgr._Instance.Frame,gameRiseType,() =>
                            {
                                gameRiseType = 2;
                                GenNewRowBlocksMultiplayer(genNewRowCount);
                                //压力块的Row也更新+1
                                for (int i = 0; i < pressureBlocks.Count; i++)
                                {
                                    pressureBlocks[i].Row++;
                                }
                            });
                        }
                        else
                        {
                            newRowBlockDatas = GenNewRowDataSinglePlayer();
                            GenNewRowBlocksSinglePlayer(newRowBlockDatas,genNewRowCount);
                            //压力块的Row也更新+1
                            for (int i = 0; i < pressureBlocks.Count; i++)
                            {
                                pressureBlocks[i].Row++;
                            }
                        }
                    }

                    boards.transform.localPosition += new Vector3(0, 1, 0);
                }
            }
        }

        //设置chain结束
        public void ChainEnd()
        {
            bool chain = false;
            foreach (var block in blockMatrix)
            {
                if (block != null)
                {
                    if (block.Chain)
                        chain = true;
                }
            }
            
            if (!chain)
            {
                chainCount = 1;
            }
        }
        
        #endregion
    }
}
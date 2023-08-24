﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Project;
using FrameWork.Manager;
using ResourceLoad;
using Random = UnityEngine.Random;
using Spine.Unity;

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

        public Transform boards = null;
        public Transform pressureBoard = null;
        public float blockBoardOffsetX;
        private BlockData[] newRowBlockDatas;

        //比赛中玩家和对手用户名
        public string selfUserName;
        public string otehrUserName;
        
        #region 游戏逻辑部分

        //初始化游戏
        public void InitGame()
        {
            Application.targetFrameRate = ConstValues.targetPlatformFps;
            boards = UIManager.Inst.GetUI<GameView>(UIDef.GameView).Self_Board;
            pressureBoard = UIManager.Inst.GetUI<GameView>(UIDef.GameView).Self_PressureBoard;
            blockBoardOffsetX = UIManager.Inst.GetUI<GameView>(UIDef.GameView).SelfBlockBoardOffsetX;
            
            boards.localPosition = Vector3.zero;
            
            chainCount = 1;
            genNewRowCount = 1;
            chainCountArray.Clear();
            pressureBlocks.Clear();
        }

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


        public void FiexdUpdate()
        {
            if (!gameStart)
            {
                //genNewRowCount = 1;
                return;
            }

            UpDateBlockArea();
            
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
        private void UpDateBlockArea()
        {
            if (!BoardStopRise && !PreussUnlocking)
            {
                BoardRise(riseUpBtn);
                if (!NetManager.Instance.Multiplayer)
                {
                    OtherGameController.Inst.BoardRise(newRowBlockDatas, riseUpBtn);
                }

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
        }
        
        private int comboCount = 0;
        private int chainCount = 1;
        public List<int> chainCountArray = new List<int>();
        
        private void LateUpdateBlockArea()
        {
            if (BlocksInSameFrame.Count >= 3)
            {
                Debug.LogError("同帧率多消组合FPS-" + TimerMgr._Instance.Frame);
                comboCount = 0;
                
                
                for (int i = 0; i < BlocksInSameFrame.Count; i++)
                {
                    var targetBlock = BlocksInSameFrame[i];
                    StateManger._instance.ChangeState(BlockState.Matched, targetBlock);
                    //设置该棋子上方的棋子chain为true
                    SetUpRowBlockChain(targetBlock);
                }
                
                if (NetManager.Instance.Multiplayer)
                {
                    NetManager.Instance.GameMatched(TimerMgr._Instance.Frame,BlocksInSameFrame, 0,0,null);
                }
                
                //集合中的chain数量
                chainCount += BlocksInSameFrame.FindAll((block => block.Chain)).Count;
                comboCount = BlocksInSameFrame.Count;
                Debug.LogError(comboCount);
                
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

                    if (!NetManager.Instance.Multiplayer)
                    {
                        var otherController = OtherGameController.Inst;
                        var targetBlock_other = otherController.blockMatrix[targetBlock_self.Row, targetBlock_self.Col - 1];
                        otherController.GenComboObj(comboCount,targetBlock_other.transform.localPosition,false);
                    }
                    
                    //Combo达成，棋盘暂停移动
                    BoardStopRise = true;
                    TimerMgr._Instance.Schedule(() =>
                    {
                        BoardStopRise = false;
                        //兴建压力块
                        PressureBlock.CreatePressureBlock(true, comboCount, pressureBoard,true);

                        if (!NetManager.Instance.Multiplayer)
                        {
                            //单人模式下新建对手压力块
                            PressureBlock.CreatePressureBlock(true,comboCount,OtherGameController.Inst.pressureBoard,false);
                        }
                        
                    }, (20 * comboCount - 20) * ConstValues.fpsTime);
                }

                //chain压力块
                if (chainCount >= 2)
                {
                    GenChainObj(chainCount, BlocksInSameFrame[0].transform.localPosition);
                    chainCountArray.Add(chainCount);//chain 数量加入集合备用
                    
                    //chain达成
                    BoardStopRise = true;
                    TimerMgr._Instance.Schedule(() =>
                    {
                        BoardStopRise = false;
                        //兴建压力块，从集合中弹出chain数量
                        PressureBlock.CreatePressureBlock(false, chainCountArray[0], pressureBoard,true);
                        chainCountArray.RemoveAt(0);
                        
                        //检测chain结束
                        if (ChainEnd())
                        {
                            chainCount = 1;
                        }
                        
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

        //如果现在棋盘里所有棋子chain标签为false。那chain就结束
        public bool ChainEnd()
        {
            bool result = false;
            for (int row = 1; row <= ConstValues.MAX_ROW; row++)
            {
                for (int col = 0; col < ConstValues.MAX_COL; col++)
                {
                    var block = blockMatrix[row, col];
                    if (block.Shape != BlockShape.None)
                    {
                        if (block.Chain)
                        {
                            result = false;
                            break;
                        }
                        else
                        {
                            result = true;
                        }
                    }
                }
            }

            return result;
        }
        
        #endregion
    }
}
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
    public class OtherGameController : Controller
    {
        private static OtherGameController s_inst;

        public static OtherGameController Inst
        {
            get
            {
                if (s_inst == null)
                {
                    s_inst = new OtherGameController();
                }

                return s_inst;
            }
        }
        
        public bool gameStart = false; //游戏开始标志

        //所有stage的样式的配置
        public List<string[]> stageConfigs = new List<string[]>
        {
            ConstValues.stage_1, ConstValues.stage_2
        };

        public Transform boards = null;
        public Transform pressureBoard = null;


        #region 游戏逻辑部分

        //初始化游戏
        public void InitGame()
        {
            boards = UIManager.Inst.GetUI<GameView>(UIDef.GameView).Other_Board;
            pressureBoard = UIManager.Inst.GetUI<GameView>(UIDef.GameView).Other_PressureBoard;
            
            boards.localPosition = Vector3.zero;
            
            chainCount = 1;
            genNewRowCount = 1;
            ClearPressureData();
            // //根据数据构建所有棋子obj
            //GenBlocks(blockDatas, gameView.Self_BlockBoard);
            // gameStart = true;
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
                // genNewRowCount = 1;
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

        //更新棋盘区域逻辑
        private void UpDateBlockArea()
        {
            // if (!NetManager.Instance.Multiplayer)
            // {
            //     if (!BoardStopRise && !PreussUnlocking)
            //         BoardRise(riseUpBtn);
            //
            // }
            //
            // //检测每个block的自有逻辑
            // for (int row = 0; row < ConstValues.MAX_MATRIX_ROW; row++)
            // {
            //     for (int col = 0; col < ConstValues.MAX_COL; col++)
            //     {
            //         var block = blockMatrix[row, col];
            //         if (block != null)
            //         {
            //             block.LogicUpdate();
            //         }
            //     }
            // }
            //
            // //检测每个压力块的自有逻辑
            // for (int i = 0; i < pressureBlocks.Count; i++)
            // {
            //     pressureBlocks[i].LogicUpdate();
            // }
        }


        private int comboCount = 0;
        private int chainCount = 0;

        private void LateUpdateBlockArea()
        {
            // if (BlocksInSameFrame.Count > 0)
            // {
            //     Debug.LogError("同帧率多消组合FPS-" + TimerMgr._Instance.Frame);
            //     comboCount = 0;
            //     chainCount = 0;
            //
            //     for (int i = 0; i < BlocksInSameFrame.Count; i++)
            //     {
            //         for (int j = 0; j < BlocksInSameFrame[i].Count; j++)
            //         {
            //             comboCount++;
            //             var block = BlocksInSameFrame[i][j];
            //             if (block.Chain)
            //                 chainCount++;
            //
            //             for (int k = 0; k < pressureBlocks.Count; k++)
            //             {
            //                 var pressureBlock = pressureBlocks[k];
            //                 pressureBlock.UnlockPressureBlock(block.Row, block.Col);
            //             }
            //         }
            //     }
            //
            //     // combo压力块
            //     if (comboCount >= 4) //原数字是4，暂时换3测试
            //     {
            //         GenComboObj(comboCount, BlocksInSameFrame[0][0].transform.localPosition);
            //
            //         //Combo达成，棋盘暂停移动
            //         BoardStopRise = true;
            //         TimerMgr._Instance.Schedule(() =>
            //         {
            //             BoardStopRise = false;
            //             //兴建压力块
            //             PressureBlock.CreatePressureBlock(true, comboCount, pressureBoard);
            //         }, (20 * comboCount - 20) * ConstValues.fpsTime);
            //     }
            //
            //     //chain压力块
            //     if (chainCount >= 2)
            //     {
            //         GenChainObj(chainCount, BlocksInSameFrame[0][0].transform.localPosition);
            //
            //         //chain达成
            //         BoardStopRise = true;
            //         TimerMgr._Instance.Schedule(() =>
            //         {
            //             Debug.LogError("生成chain的压力块");
            //             BoardStopRise = false;
            //             //兴建压力块
            //             PressureBlock.CreatePressureBlock(false, chainCount, pressureBoard);
            //         }, (20 * chainCount + 80) * ConstValues.fpsTime);
            //     }
            //
            //     BlocksInSameFrame.Clear();
            // }
        }

        //棋盘上升
        public void BoardRise(BlockData[] newBlockData, bool btnRise = false)
        {
            //TODO 到达顶部，就不上升了
            if (btnRise)
            {
                float index = (int)(boards.transform.localPosition.y / ConstValues.OTHER_BLOCK_Y_OFFSET);
                boards.transform.localPosition = new Vector3(0, (index + 1) * ConstValues.OTHER_BLOCK_Y_OFFSET, 0);
            }
            else
            {
                if (TimerMgr._Instance.Frame % ConstValues.Rise_Times[7] == 0)
                {
                    if (boards.transform.localPosition.y % ConstValues.OTHER_BLOCK_Y_OFFSET == 0)
                    {
                        if (NetManager.Instance.Multiplayer)
                        {
                            // NetManager.Instance.GameRaiseReq(TimerMgr._Instance.Frame, () =>
                            // {
                            //     GenNewRowBlocksMultiplayer(genNewRowCount,false);
                            //     //压力块的Row也更新+1
                            //     for (int i = 0; i < pressureBlocks.Count; i++)
                            //     {
                            //         pressureBlocks[i].Row++;
                            //     }
                            // });
                        }
                        else
                        {
                            GenNewRowBlocksSinglePlayer(newBlockData,genNewRowCount,false);
                            //压力块的Row也更新+1
                            for (int i = 0; i < pressureBlocks.Count; i++)
                            {
                                pressureBlocks[i].Row++;
                            }
                        }
                    }
                    
                    //对手棋子和己方棋子移动比例
                    float scale = (float)ConstValues.OTHER_BLOCK_Y_OFFSET / ConstValues.SELF_BLOCK_Y_OFFSET;
                    boards.transform.localPosition += new Vector3(0, 1 * scale, 0);
                }
            }
        }

        #endregion
    }
}
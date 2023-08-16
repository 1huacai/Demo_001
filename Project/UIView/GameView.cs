using Demo.Tools;
using UnityEngine;
using UnityEngine.UI;

namespace Demo
{
    public class GameView : UIBase
    {
        private Transform self_Board;//玩家棋盘
        private Transform other_Board;// 对手棋盘
        
        private Transform self_BlockBoard;//玩家block棋盘
        private Transform other_BlockBoard;//对手block棋盘
        
        private Transform self_PressureBoard;//玩家压力块棋盘
        private Transform other_PressureBoard;//对手压力块棋盘
        
        private Transform self_EffectArea;//玩家特效特效区域
        private Transform other_EffectArea;//对手特效区域
        
        private Button reGenBlockBtn;
        private Button riseBoardBtn;
        private Button settingBtn;

        private float selfBlockBoardOffsetX;//玩家棋盘横向动态偏移
        #region Get/Set

        public Transform Self_Board
        {
            get { return self_Board; }
        }

        public Transform Other_Board
        {
            get { return other_Board; }
        }

        public Transform Self_BlockBoard
        {
            get { return self_BlockBoard; }
        }

        public Transform Other_BlockBoard
        {
            get { return other_BlockBoard; }
        }

        public Transform Self_PressureBoard
        {
            get { return self_PressureBoard; }
        }

        public Transform Other_PressureBoard
        {
            get { return other_PressureBoard; }
        }


        public Transform Self_EffectArea
        {
            get { return self_EffectArea; }
        }

        public Transform Other_EffectArea
        {
            get { return other_EffectArea; }
        }

        public float SelfBlockBoardOffsetX
        {
            get { return selfBlockBoardOffsetX; }
        }
        
        #endregion
        
        
        
        public override void InitUI(params object[] msg)
        {
            //自己棋盘
            self_Board = transform.Find("PlayerAreas/SelfPlayerArea/Board");
            self_BlockBoard = self_Board.Find("BlockBoard");
            self_PressureBoard = self_Board.Find("PressureBoard");
            self_EffectArea = self_Board.Find("EffectArea");
            selfBlockBoardOffsetX = (Screen.width - self_BlockBoard.GetComponent<RectTransform>().sizeDelta.x)/2f;

            //对手棋盘
            other_Board = transform.Find("PlayerAreas/OtherPlayerArea/Board");
            other_BlockBoard = self_Board.Find("BlockBoard");
            other_PressureBoard = self_Board.Find("PressureBoard");
            other_EffectArea = self_Board.Find("EffectArea");
            
            
            reGenBlockBtn = transform.Find("ReGenBlockBtn").GetComponent<Button>();
            riseBoardBtn = transform.Find("RiseBoardBtn").GetComponent<Button>();
            settingBtn = transform.Find("SettingBtn").GetComponent<Button>();
            SelfGameController.Inst.InitGame();
        }

        public override void RefreshShow(params object[] msg)
        {
            
        }

        public override void RegisterEvent()
        {
            reGenBlockBtn.onClick.RemoveAllListeners();
            reGenBlockBtn.onClick.AddListener(ReGenBlockBtnCallback);
            riseBoardBtn.onClick.RemoveAllListeners();
            riseBoardBtn.onClick.AddListener(() => { SelfGameController.Inst.riseUpBtn = true; });
            settingBtn.onClick.RemoveAllListeners();
            settingBtn.onClick.AddListener(()=>{ Debug.LogError("打开设置界面"); });
        }

        public override void UnRegisterEvent()
        {
           
        }

        public override void Destroy()
        {
           
        }
        
        //重新刷新棋盘按钮回调
        private void ReGenBlockBtnCallback()
        {
            DestroyAllBlocks();
        }
        
        public void DestroyAllBlocks()
        {
            //预先清空所有计时器
            TimerMgr._Instance.RemoveAllTimer();
            
            for (int i = 0; i < SelfGameController.Inst.blockMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < SelfGameController.Inst.blockMatrix.GetLength(1); j++)
                {
                    if (SelfGameController.Inst.blockMatrix[i, j] != null)
                    {
                        GameObject.Destroy(SelfGameController.Inst.blockMatrix[i,j].gameObject);
                        SelfGameController.Inst.blockMatrix[i, j] = null;
                    }
                }
            }

            for (int i = 0; i < SelfGameController.Inst.pressureBlocks.Count; i++)
            {
                var pressureBlockObj = SelfGameController.Inst.pressureBlocks[i].gameObject;
                Destroy(pressureBlockObj);
            }
            SelfGameController.Inst.pressureBlocks.Clear();
            
            
            SelfGameController.Inst.GenNewRowCount = 1;
            self_Board.localPosition = Vector3.zero;
            var blockDatas = SelfGameController.Inst.GenBlockDatas(SelfGameController.Inst.stageConfigs,4);
            //根据数据构建所有棋子obj
            SelfGameController.Inst.GenBlocks(blockDatas,self_BlockBoard);
        }

    }
}
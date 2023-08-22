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

        private Transform self_UserPart;
        private Transform other_Userpart;

        private Text self_UserName;
        private Text other_UserName;
        
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
            self_UserPart = transform.Find("MyUserPart");
            self_UserName = self_UserPart.Find("UserName").GetComponent<Text>();
            selfBlockBoardOffsetX = (Screen.width - self_BlockBoard.GetComponent<RectTransform>().sizeDelta.x)/2f;
            
            
            //对手棋盘
            other_Board = transform.Find("PlayerAreas/OtherPlayerArea/Board");
            other_BlockBoard = other_Board.Find("BlockBoard");
            other_PressureBoard = other_Board.Find("PressureBoard");
            other_EffectArea = other_Board.Find("EffectArea");
            other_Userpart = transform.Find("OtherUserPart");
            other_UserName = other_Userpart.Find("UserName").GetComponent<Text>();
            
            reGenBlockBtn = transform.Find("ReGenBlockBtn").GetComponent<Button>();
            riseBoardBtn = transform.Find("RiseBoardBtn").GetComponent<Button>();
            settingBtn = transform.Find("SettingBtn").GetComponent<Button>();
            
            //单人模式下 TODO 单人模式下己方和敌方棋盘生成后期需要修改
            if (!NetManager.Instance.Multiplayer)
            {
                //单人模式下初始化游戏
                SinglePlayerInitGame();
            }
            else
            {
                MultiplayerInitGame();
            }
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
        
        //单人
        public void SinglePlayerInitGame()
        {
            var selfController = SelfGameController.Inst;
            selfController.InitGame();
                
            //构建己方棋子
            var blockDatas =  selfController.GenBlockDatas(selfController.stageConfigs, 4);
            selfController.GenBlocks(blockDatas, Self_BlockBoard,true);
            StateManger._instance.Init(selfController);
            TimerMgr._Instance.Init();
                
            //构建对手棋子（镜像）
            var otherController = OtherGameController.Inst;
            otherController.InitGame();
            otherController.GenBlocks(blockDatas, other_BlockBoard,false);

            selfController.gameStart = true;
        }
        
        //多人
        public void MultiplayerInitGame()
        {
            
            var selfController = SelfGameController.Inst;
            selfController.InitGame();
                
            //显示玩家和对手用户名
            SetMultiplayerInfo(selfController.selfUserName, selfController.otehrUserName);
            
            //构建己方棋子
            var blockBuffer = selfController.blockBufferWithNet;
            var blockDatas =  selfController.GenBlockDatasWith(blockBuffer);
            selfController.GenBlocks(blockDatas, Self_BlockBoard,true);
            StateManger._instance.Init(selfController);
            TimerMgr._Instance.Init();
                
            //构建对手棋子（镜像）
            var otherController = OtherGameController.Inst;
            otherController.InitGame();
            otherController.GenBlocks(blockDatas, other_BlockBoard,false);

            selfController.gameStart = false;
        }
        
        //重新刷新棋盘按钮回调
        private void ReGenBlockBtnCallback()
        {
            ReStartGame();
        }
        
        public void ReStartGame()
        {
            if(NetManager.Instance.Multiplayer)
                return;
            
            TimerMgr._Instance.RemoveAllTimer();
            var selfController = SelfGameController.Inst;
            selfController.ClearAllBlockObj();

            selfController.InitGame();
            StateManger._instance.Init(selfController);
            TimerMgr._Instance.Init();
            var blockDatas = selfController.GenBlockDatas(SelfGameController.Inst.stageConfigs,4);
            //根据数据构建所有棋子obj
            selfController.GenBlocks(blockDatas,self_BlockBoard);
            
            var otherController = OtherGameController.Inst;
            otherController.ClearAllBlockObj();
            otherController.InitGame();
            otherController.GenBlocks(blockDatas, other_BlockBoard,false);
            
        }

        public void SetMultiplayerInfo(string self_name,string other_name)
        {
            self_UserName.text = self_name;
            other_UserName.text = other_name;
        }
        
        
    }
}
using UnityEngine;
using FrameWork.Manager;
using FrameWork.Audio;

namespace Demo
{
    public class GameView : UIBase
    {
        private Transform boards;//棋盘
        private Transform blockBoard;//block棋盘
        private Transform pressureBoard;//压力块棋盘
        private Transform effectArea;//特效区域

        public Transform Boards
        {
            get { return boards; }
        }
        
        public Transform BlockBoard
        {
            get { return blockBoard; }
        }

        public Transform PressureBoard
        {
            get { return pressureBoard; }
        }

        public Transform EffectArea
        {
            get { return effectArea; }
        }
        
        
        
        private AorButton reGenBlockBtn;
        
        public override void InitUI(params object[] msg)
        {
            boards = transform.Find("PlayerArea/Boards");
            blockBoard = boards.Find("BlockBoard");
            pressureBoard = boards.Find("PressureBoard");
            effectArea = boards.Find("EffectArea");
            reGenBlockBtn = transform.Find("PlayerArea/ReGenBlockBtn").GetComponent<AorButton>();
            
            GameManger.Inst.InitGame();
        }

        public override void RefreshShow(params object[] msg)
        {
            
        }

        public override void RegisterEvent()
        {
            reGenBlockBtn.onClick.RemoveAllListeners();
            reGenBlockBtn.onClick.AddListener(ReGenBlockBtnCallback);
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
            for (int i = 0; i < GameManger.Inst.blockMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < GameManger.Inst.blockMatrix.GetLength(1); j++)
                {
                    if (GameManger.Inst.blockMatrix[i, j] != null)
                    {
                        GameObject.Destroy(GameManger.Inst.blockMatrix[i,j].gameObject);
                        GameManger.Inst.blockMatrix[i, j] = null;
                    }
                }
            }
            
            var blockDatas = GameManger.Inst.GenBlockDatas(4);
            //根据数据构建所有棋子obj
            GameManger.Inst.GenBlocks(blockDatas,blockBoard);
        }

    }
}
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

        public override void InitUI(params object[] msg)
        {
            boards = transform.Find("PlayerArea/Boards");
            blockBoard = boards.Find("BlockBoard");
            pressureBoard = boards.Find("PressureBoard");
            
            var blockDatas = GameManger.Inst.GenBlockDatas();
            //根据数据构建所有棋子obj
            GameManger.Inst.GenBlocks(blockDatas,blockBoard);
            
            // Debug.LogError($"棋子个数{blockDatas.Count}");
            // for (int i = 0; i < blockDatas.Count; i++)
            // {
            //     Debug.LogError($"{blockDatas[i].row}----{blockDatas[i].col}---{blockDatas[i].type}");
            // }

        }

        public override void RefreshShow(params object[] msg)
        {
            
        }

        public override void RegisterEvent()
        {
            
        }

        public override void UnRegisterEvent()
        {
           
        }

        public override void Destroy()
        {
           
        }
    }
}
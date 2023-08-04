using System.Collections;
using FrameWork.Audio;
using FrameWork.Manager;
using UnityEngine;
using Project;

namespace Demo
{
    public class LoginView : UIBase
    {
        AorButton m_singlePlayerBtn;
        private AorButton m_multiPlayerBtn;

        public override void InitUI(params object[] msg)
        {
            m_singlePlayerBtn = transform.Find("SinglePlayerBtn").GetComponent<AorButton>();
            m_multiPlayerBtn = transform.Find("MultiPlayerBtn").GetComponent<AorButton>();
            
            RefreshShow();
            NetManager.Instance.Init();
            NetManager.Instance.ConnetServer();
        }

        public override void RefreshShow(params object[] msg)
        {
            SingletonManager.GetManager<AudioManager>().PlayMusic("Sound/BG/City_Autumn_BG");
        }

        public override void RegisterEvent()
        {
            m_singlePlayerBtn.onClick.RemoveAllListeners();
            m_singlePlayerBtn.onClick.AddListener(SinglePlayerGame);
            
            m_multiPlayerBtn.onClick.RemoveAllListeners();
            m_multiPlayerBtn.onClick.AddListener(MultiPlayerGame);
        }

        public override void UnRegisterEvent()
        {
        }

        public override void Destroy()
        {
        }
        
        private void CloseUI()
        {
            UIManager.Inst.CloseUI(UIDef.LoginView);
        }

        #region 按钮回调
        
        //单人游戏
        private void SinglePlayerGame()
        {
            Debug.LogError("单人游戏");
            UIManager.Inst.OpenUI(UIDef.GameView);
            CloseUI();
        }
        
        //多人游戏
        private void MultiPlayerGame()
        {
            Debug.LogError("多人游戏");
            // if (NetCore.connected == false)
            // {
            //     Debug.LogError("开始链接");
            //     NetManager.Instance.Server_Logined = false;
            //     NetCore.Connect(ConstValues.serverIp, ConstValues.serverPort, () =>
            //     {
            //         Debug.Log("connect server success");
            //     });
            // }
            // else
            // {
            //     Debug.LogError("链接成功");
            //     NetManager.Instance.Server_Logined = true;
            //     //TODO 链接服务器
            //     NetManager.Instance.GameBattle();
            // }
            
            
            // UIManager.Inst.OpenUI(UIDef.GameView);
            // CloseUI();
        }
        #endregion
        
    }
}

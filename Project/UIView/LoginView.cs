using System.Collections;
using FrameWork.Audio;
using FrameWork.Manager;
using UnityEngine;
using UnityEngine.SceneManagement;

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
            // UIManager.Inst.OpenUI(UIDef.GameView);
            // CloseUI();
        }
        #endregion
        
    }
}

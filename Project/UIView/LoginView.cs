using System;
using System.Collections;
using FrameWork.Audio;
using FrameWork.Manager;
using UnityEngine;
using UnityEngine.UI;
using Project;

namespace Demo
{
    public class LoginView : UIBase
    {
        private Button m_singlePlayerBtn;
        private Button m_multiPlayerBtn;
        private GameObject matchWindowObj;
        private Button cancelMatchBtn;
        private Text matchTimeText;
        
        private float curTime = 0f;
        private float matchTime = 0;
        private bool matchStart = false;
        
        public override void InitUI(params object[] msg)
        {
            m_singlePlayerBtn = transform.Find("SinglePlayerBtn").GetComponent<Button>();
            m_multiPlayerBtn = transform.Find("MultiPlayerBtn").GetComponent<Button>();
            matchWindowObj = transform.Find("MatchWindow").gameObject;
            cancelMatchBtn = matchWindowObj.transform.Find("CancelMatchBtn").GetComponent<Button>();
            matchTimeText = matchWindowObj.transform.Find("MatchTime").GetComponent<Text>();
            
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
            
            cancelMatchBtn.onClick.RemoveAllListeners();
            cancelMatchBtn.onClick.AddListener(CancelMatch);
        }
        
        protected void Update()
        {
            //匹配计时
            if (matchStart)
            {
                curTime += Time.deltaTime;
                if (curTime >= 1f)
                {
                    curTime = 0;
                    matchTime++;
                    matchTimeText.text = matchTime.ToString();
                }
            }
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
            matchWindowObj.SetActive(true);
            NetManager.Instance.MatchStartReq(() =>
            {
                matchStart = true;
            });
            // NetManager.Instance.GameBattle(() =>
            // {
            //     matchStart = true;
            // });
        }
        
        private void CancelMatch()
        {
            matchStart = false;
            curTime = 0;
            matchTime = 0;
            matchTimeText.text = matchTime.ToString();
            matchWindowObj.SetActive(false);
            NetManager.Instance.MatchCancelReq();
        }
        #endregion
        
        //匹配成功后进入游戏初始化
        public void GameReadyEnterGame()
        {
            matchWindowObj.SetActive(false);
            UIManager.Inst.OpenUI(UIDef.GameView);
            CloseUI();
        }
        
        
    }
}

using FrameWork.Audio;
using FrameWork.Manager;
using UnityEngine;

namespace Demo
{
    public class LoginView : UIBase
    {
        AorImage m_Sprite;
        AorButton m_closeBtn;
        private void CloseUI()
        {
            m_Sprite.LoadImage("Ui/BagAtlas/bb_frm_001");
        }

        public override void InitUI(params object[] msg)
        {
            m_closeBtn = transform.Find("Button").GetComponent<AorButton>();
            m_Sprite = transform.Find("Sprite").GetComponent<AorImage>();

            RefreshShow();
        }

        public override void RefreshShow(params object[] msg)
        {
            SingletonManager.GetManager<AudioManager>().PlayMusic("Sound/BG/City_Autumn_BG");
        }

        public override void RegisterEvent()
        {
            m_closeBtn.onClick.RemoveAllListeners();
            m_closeBtn.onClick.AddListener(CloseUI);
        }

        public override void UnRegisterEvent()
        {
        }

        public override void Destroy()
        {
        }
    }
}

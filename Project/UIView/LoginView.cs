using FrameWork.Audio;
using FrameWork.Manager;
using UnityEngine;

namespace Demo
{
    public class LoginView : MonoBehaviour
    {
        AorImage m_Sprite;
        AorButton m_closeBtn;
        private void Awake()
        { 
            m_closeBtn = transform.Find("Button").GetComponent<AorButton>();
            m_Sprite = transform.Find("Sprite").GetComponent<AorImage>();
        }
        private void Start()
        {
            SingletonManager.GetManager<AudioManager>().PlayMusic("Sound/BG/City_Autumn_BG");
            m_closeBtn.onClick.AddListener(CloseUI);
        }
        private void CloseUI()
        {
            m_Sprite.LoadImage("Ui/BagAtlas/bb_frm_001");

            //AorText aorText = transform.gameObject.AddComponent<AorText>();
            //aorText.text = Lang.get("test1");
            //UIManager.Inst.CloseUI(UIDef.LoginMainView);
        }
    }
}

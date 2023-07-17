using UnityEngine;
using FrameWork.App;
using System.Collections.Generic;
using FrameWork.Manager;
using ResourceLoad;
using System;

namespace Demo
{
    public class UIManager
    {
        private static UIManager s_inst;
        public static UIManager Inst
        {
            get
            {
                if (s_inst == null)
                    s_inst = new UIManager();
                return s_inst;
            }
        }

        public UIManager()
        {
            var _main_go = GameObject.Find("UISystemRoot/AorUICanvas#/AorUIStage#/MainLayer");
            if (_main_go)
                m_MainUI = _main_go.transform;
            var _top_go = GameObject.Find("UISystemRoot/AorUICanvas#/AorUIStage#/TopLayer");
            if (_top_go)
                m_TopUI = _top_go.transform;
            var _back_go = GameObject.Find("UISystemRoot/AorUICanvas#/UIBackLayer#");
            if (_back_go)
                m_BackUI = _back_go.transform;
        }

        private Dictionary<UIDef, UIBase> UIDic = new Dictionary<UIDef, UIBase>();

        public Transform m_MainUI;
        public Transform m_TopUI;
        public Transform m_BackUI;

        public void OpenUI(UIDef name,params object[] msg) 
        {
            if (!UIDic.ContainsKey(name))
            {
                SingletonManager.GetManager<ResourcesManager>().LoadPrefabInstance("TSUprefabs/" + name.ToString(), (obj) =>
                {
                    if (null == obj)
                    {
                        return;
                    }
                    var _UIbase = obj.GetComponent(name.ToString());
                    if (_UIbase == null)
                    {
                        _UIbase = obj.AddComponent(Type.GetType(name.ToString()));
                    }
                    var uibase = (UIBase)_UIbase;
                    UIDic[name] = uibase;
                    uibase.InitUI(msg);
                    uibase.RegisterEvent();
                    if (name == UIDef.LoginView)
                        obj.transform.SetParent(m_BackUI, false);
                    else
                        obj.transform.SetParent(m_MainUI, false);
                }, false);
            }
            else
            {
                var uibase = UIDic[name];
                uibase.RefreshShow(msg);
                uibase.RegisterEvent();
            }
        }
        public void CloseUI(UIDef name)
        {
            if (UIDic.ContainsKey(name))
            {
                var uibase = UIDic[name];
                uibase.UnRegisterEvent();
                uibase.Destroy();
                GameObject.Destroy(UIDic[name].gameObject);
                UIDic.Remove(name);
            }
        }
    }
}

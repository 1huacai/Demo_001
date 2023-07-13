using UnityEngine;
using FrameWork.App;
using System.Collections.Generic;
using FrameWork.Manager;
using ResourceLoad;

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
        private Dictionary<UIDef, GameObject> UIDic = new Dictionary<UIDef, GameObject>();
        public void OpenUI(UIDef name)
        {
            SingletonManager.GetManager<ResourcesManager>().LoadPrefabInstance("TSUprefabs/" + name.ToString(), (obj) =>
            {
                if (null == obj)
                {
                    return;
                }
                UIDic[name] = obj;
                obj.transform.SetParent(GameObject.Find("UISystemRoot/AorUICanvas#/UIBackLayer#").transform, false);
            },false);
        }
        public void CloseUI(UIDef name)
        {
            GameObject.Destroy(UIDic[name]);
        }
    }
}

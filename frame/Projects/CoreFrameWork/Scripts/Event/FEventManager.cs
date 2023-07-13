
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CoreFrameWork.Event
{
    public class FEventManager : SingletonManager<FEventManager>
    {
        public Dictionary<Enum, int> MonitorDic = null;


        /// <summary>
        /// 主消息监听、分发器
        /// </summary>
        public FGameEvent MainEvent;
        private Dictionary<string, FGameEvent> m_FGameEventDic = new Dictionary<string, FGameEvent>();


        public void Awake()
        {
            MainEvent = gameObject.AddComponent<FGameEvent>();
            DontDestroyOnLoad(gameObject);
        }
 

        public FGameEvent AddFGameEvent(string name)
        {
            FGameEvent _FGameEvent = null;
            if (string.IsNullOrEmpty(name))
            {
                return _FGameEvent;
            }

            if (!m_FGameEventDic.ContainsKey(name))
            {
                GameObject _go = new GameObject(name);
                _go.transform.SetParent(transform);
                _FGameEvent = _go.AddComponent<FGameEvent>();
                m_FGameEventDic.Add(name, _FGameEvent);
            }
            else
            {
                _FGameEvent = m_FGameEventDic[name];
            }
            return _FGameEvent;
        }

        public void RemoveFGameEvent(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            if (m_FGameEventDic.ContainsKey(name))
            {
                FGameEvent _FGameEvent = m_FGameEventDic[name];
                m_FGameEventDic.Remove(name);
                _FGameEvent.Dispose();
                GameObject.Destroy(_FGameEvent);
            }
        }



        public override void Dispose()
        {
            m_FGameEventDic = null;

        }
    }
}

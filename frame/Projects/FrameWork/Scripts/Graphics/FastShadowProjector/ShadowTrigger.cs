using UnityEngine;
using System;
using System.Collections;
using FrameWork.App;
using FrameWork.Manager;

namespace FrameWork.Graphics.FastShadowProjector
{

    //[AddComponentMenu("Fast Shadow Projector/Shadow Trigger")]
    public class ShadowTrigger : MonoBehaviour
    {

        public Action OnShadowEnter;
        public Action OnShadowStay;
        public Action OnShadowExit;

        bool _InShadow = false;

        public bool DetectShadow
        {
            set
            {
                _DetectShadow = value;
            }

            get
            {
                return _DetectShadow;
            }
        }

        [UnityEngine.SerializeField]
        bool _DetectShadow;

        public bool DetectLight
        {
            set
            {
                _DetectLight = value;
            }

            get
            {
                return _DetectLight;
            }
        }

        [UnityEngine.SerializeField]
        bool _DetectLight;

        void Awake()
        {
        }

        void Start()
        {
            AddShadowTrigger();
        }

        void OnEnable()
        {
            AddShadowTrigger();
        }

        void OnDisable()
        {
            RemoveShadowTrigger();
        }

        void OnDestroy()
        {
            RemoveShadowTrigger();
        }

        void AddShadowTrigger()
        {

            SingletonManager.GetManager<GraphicsManager>().ShadowMgr.AddShadowTrigger(this);
           
        }

        void RemoveShadowTrigger()
        {

            SingletonManager.GetManager<GraphicsManager>().ShadowMgr.RemoveShadowTrigger(this);
       
        }

        public void OnTriggerCheckDone(bool pointInShadow)
        {
            if (pointInShadow)
            {

                if (!_InShadow)
                {
                    _InShadow = true;

                    if (OnShadowEnter != null)
                    {
                        OnShadowEnter();
                    }
                }
                else if (OnShadowStay != null)
                {
                    OnShadowStay();
                }
            }
            else
            {

                if (_InShadow)
                {

                    _InShadow = false;

                    if (OnShadowExit != null)
                    {
                        OnShadowExit();
                    }
                }
            }

        }
    }
}


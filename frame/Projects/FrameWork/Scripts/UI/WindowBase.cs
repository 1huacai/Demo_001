using System.Collections;
using System.Collections.Generic;
using UnityEngine;




namespace FrameWork
{
    public class WindowBase : MonoBehaviour
    {

        public object[] Args;
        protected Transform m_Transform;

        /// <summary>
        /// 初始化
        /// </summary>
        public virtual void Initialization()
        {

        }

        protected virtual void Awake()
        {
            m_Transform = transform;


        }



        // Use this for initialization
        protected virtual void Start()
        {


        }

        protected virtual void OnEnable()
        {

        }
        protected virtual void OnDisable()
        {

        }
        protected virtual void OnDestroy()
        {

        }

        protected virtual void OnClose()
        {

        }
    }
}
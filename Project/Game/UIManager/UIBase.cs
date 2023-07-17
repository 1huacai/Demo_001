using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Demo
{
    public abstract class UIBase : MonoBehaviour
    {
        public abstract void InitUI(params object[] msg);
        public abstract void RefreshShow(params object[] msg);
        public abstract void RegisterEvent();
        public abstract void UnRegisterEvent();
        public abstract void Destroy();
    }
}

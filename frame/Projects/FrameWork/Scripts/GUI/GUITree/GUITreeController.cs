using System;
using FrameWork.App;


namespace  FrameWork.GUI
{
    public abstract class GuiTreeController : MonoSwitch
    {
        private object _Component;

        public object Component
        {
            get { return _Component; }
        }
    

        public virtual void Init(object c)
        {
            _Component = c;
        }

    }

}

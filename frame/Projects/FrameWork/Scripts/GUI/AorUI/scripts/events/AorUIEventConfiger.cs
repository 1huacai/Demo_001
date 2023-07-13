using UnityEngine;
using System.Collections;
using FrameWork.App;

namespace FrameWork.GUI.AorUI.events
{
    /// <summary>
    /// AorUIEventConfig 关于AorUIEventListener所需的各种全局设置
    /// </summary>
    public class AorUIEventConfig
    {
        /// <summary>
        /// 按下到弹起的时间限制,超过这个限制后则不触发Click事件(毫秒)
        /// </summary>
        public static int ClickDelay = 500;
        /// <summary>
        /// 按下到弹起的距离阀值,超过这个值后则不认为是Click事件,这个值同样影响长按事件的逻辑判断;(屏幕像素)
        /// </summary>
        public static int PosThreshold = 50;
        /// <summary>
        /// 按下后持续时间超过此阀值时,将触发长按事件(毫秒)
        /// </summary>
        public static int LongPressTimeThreshold = 1000;
        /// <summary>
        /// 按下到弹起的时间限制,超过这个限制后则不触发Swing事件(毫秒)
        /// </summary>
        public static int SwingTimeThreshold = 600;
        /// <summary>
        /// 按下到弹起的距离阀值,小于此阀值则不触发Swing事件(毫秒)
        /// </summary>
        public static int SwingThreshold = 60;
    }

    /// <summary>
    /// AorUIEventConfiger 一个用于设置AorUIEventConfig的MonoBehaviour组件. 设置完毕后,会自行移除自身.(程序员应该用不上此组件)
    /// </summary>
    public class AorUIEventConfiger : MonoSwitch
    {

        public int ClickDelay = 100;
        public int PosThreshold = 50;
        public int LongPressTimeThreshold = 1000;
        public int SwingTimeThreshold = 600;
        public int SwingThreshold = 60;

        public override void OnAwake()
        {
            base.OnAwake();

            AorUIEventConfig.ClickDelay = ClickDelay;
            AorUIEventConfig.PosThreshold = PosThreshold;
            AorUIEventConfig.LongPressTimeThreshold = LongPressTimeThreshold;
            AorUIEventConfig.SwingTimeThreshold = SwingTimeThreshold;
            AorUIEventConfig.SwingThreshold = SwingThreshold;
        }

        protected override void Initialization()
        {
            base.Initialization();

            GameObject.Destroy(this);
        }
    }
}


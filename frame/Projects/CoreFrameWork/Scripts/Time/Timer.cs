using System;

namespace CoreFrameWork.TimeTool
{
    public class Timer
    {
        #region 静态公有域

        #endregion

        #region 静态私有域

        #endregion

        #region 静态保护域

        #endregion

        #region 公有域
        #endregion

        #region 私有域
        /// <summary>
        /// 回调函数
        /// </summary>
        private Action _call;
        /// <summary>
        /// 延迟时间：秒
        /// </summary>
        private float _delay;
        /// <summary>
        /// 结束时间 (必须采用double，float精度不够)
        /// </summary>
        private double _endTime;
        /// <summary>
        /// 是否执行一次
        /// </summary>
        private bool _once;
        /// <summary>
        /// 忽略缩放
        /// </summary>
        private bool _ignoreScale;
        /// <summary>
        /// 自定义类型参数
        /// </summary>
        private int _customParam;

        #endregion

        #region 保护域

        #endregion

        #region 静态属性

        #endregion

        #region 公有属性
        /// <summary>
        /// 回调函数
        /// </summary>
        public Action Call
        {
            get
            {
                return _call;
            }
        }

        /// <summary>
        /// 延迟时间：秒
        /// </summary>
        public float Delay
        {
            get
            {
                return _delay;
            }
        }

        /// <summary>
        /// 是否执行一次
        /// </summary>
        public bool Once
        {
            get
            {
                return _once;
            }
        }

        /// <summary>
        /// 忽略缩放
        /// </summary>
        public bool IgnoreScale
        {
            get
            {
                return _ignoreScale;
            }
        }
        #endregion
        /// <summary>
        /// 自定义类型参数
        /// </summary>
        public int customParam
        {
            get
            {
                return _customParam;
            }
        }

        #region 私有属性

        #endregion

        #region 保护属性

        #endregion

        #region 静态公有方法

        #endregion

        #region 静态私有方法

        #endregion

        #region 静态保护方法

        #endregion

        #region 公有方法
        /// <summary>
        /// 构造函数
        /// </summary>
        public Timer(Action call, float delay, bool once = false, bool ignoreScale = true, int _customParam = 0)
        {
            this._call = call;
            this._delay = delay;
            this._once = once;
            this._ignoreScale = ignoreScale;
            this._customParam = _customParam;
        }

        /// <summary>
        /// 重置，重新计算时长
        /// </summary>
        public void Reset(IGetTime getTime)
        {
                _endTime = (_ignoreScale ? getTime.GetUnscaledTime() : getTime.GetTime()) + _delay;
            
        }

        /// <summary>
        /// 是否可以重置
        /// </summary>
        /// <returns></returns>
        public bool IsReset(IGetTime getTime)
        {

                return _endTime <= (_ignoreScale ? getTime.GetUnscaledTime() : getTime.GetTime());
            
        }
        #endregion

        #region 私有有方法

        #endregion

        #region 保护方法

        #endregion

        #region 重载方法

        #endregion

        #region 枚举

        #endregion
    }
}

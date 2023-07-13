namespace CoreFrameWork.TimeTool
{
    public class SystemGetTime : IGetTime
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
        /// 开始时间：毫秒
        /// </summary>
        private long _startTime;
        #endregion

        #region 保护域

        #endregion

        #region 静态属性

        #endregion

        #region 公有属性

        #endregion

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
        public SystemGetTime()
        {
            _startTime = TimeKit.CurrentTimeMillis();
        }
        /// <summary>
        /// 获取时间
        /// </summary>
        /// <returns>秒</returns>
        public float GetTime()
        {
            return (TimeKit.CurrentTimeMillis() - _startTime) / 1000f;
        }

        /// <summary>
        /// 获取未缩放时间
        /// </summary>
        /// <returns>秒</returns>
        public float GetUnscaledTime()
        {
            return (TimeKit.CurrentTimeMillis() - _startTime) / 1000f;
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

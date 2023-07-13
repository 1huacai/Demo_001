namespace CoreFrameWork.TimeTool
{
    /// <summary>
    /// 接口说明：Timer获取时间接口
    /// </summary>
    public interface IGetTime
    {
        /// <summary>
        /// 获取时间
        /// </summary>
        /// <returns>秒</returns>
        float GetTime();

        /// <summary>
        /// 获取未缩放时间
        /// </summary>
        /// <returns>秒</returns>
        float GetUnscaledTime();
    }
}

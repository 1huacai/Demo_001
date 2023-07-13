using XCore.Event;

namespace XCore.Utils
{
    public static class EventNumberUtils
    {
        /// <summary>
        /// 连接成功
        /// </summary>
        public static int CONNECT_OK = GlobalEvent.NewEventId;
        /// <summary>
        /// 连接失败
        /// </summary>
        public static int CONNECT_FAIL = GlobalEvent.NewEventId;
        /// <summary>
        /// 连接关闭
        /// </summary>
        public static int CONNECT_CLOSE = GlobalEvent.NewEventId;
        /// <summary>
        /// 连接认证成功
        /// </summary>
        public static int CONNECT_CERITY_OK = GlobalEvent.NewEventId;
        /// <summary>
        /// 数据广播
        /// </summary>
        public static int CONNECT_DATA_BROADCAST = GlobalEvent.NewEventId;
    }
}
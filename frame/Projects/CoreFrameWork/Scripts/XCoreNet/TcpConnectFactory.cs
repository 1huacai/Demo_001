using XCore.Collections;

namespace XCore.Net
{
    /// <summary>
    /// Tcp连接工厂
    /// </summary>
    public class TcpConnectFactory
    {
        /// <summary>
        /// 连接列表
        /// </summary>
        protected static readonly XCoreList<TcpConnect> LIST = new XCoreList<TcpConnect>();

        /// <summary>
        /// 获得连接
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="port">端口</param>
        /// <returns>连接</returns>
        public static TcpConnect GetConnect(string address, int port)
        {
            TcpConnect connect = Get(address, port);
            if (connect == null)
            {
                connect = new TcpConnect(address, port);
                LIST.Add(connect);
            }
            return connect;
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        /// <param name="connect">连接</param>
        public static void Close(TcpConnect connect)
        {
            LIST.Remove(connect);
            connect.Close();
        }

        /// <summary>
        /// 关闭所有连接
        /// </summary>
        public static void CloseAll()
        {
            for (int i = 0; i < LIST.Size; i++)
            {
                LIST[i].Close();
            }
            LIST.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        protected static TcpConnect Get(string address, int port)
        {
            //TODO 如果性能有大问题，可以改这个数据结构
            for (int i = LIST.Size - 1; i >= 0; i--)
            {
                if (LIST[i].Address == address && LIST[i].Port == port)
                {
                    return LIST[i];
                }
            }
            return null;
        }

        internal TcpConnectFactory()
        {
        }
    }
}

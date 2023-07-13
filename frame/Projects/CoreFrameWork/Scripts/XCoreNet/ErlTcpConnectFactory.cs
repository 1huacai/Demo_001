namespace XCore.Net
{
    public class ErlTcpConnectFactory : TcpConnectFactory
    {
        /// <summary>
        /// 获得Erl连接
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="port">端口</param>
        /// <returns>Erl连接</returns>
        public new static ErlTcpConnect GetConnect(string address, int port)
        {
            ErlTcpConnect connect = Get(address, port) as ErlTcpConnect;
            if (connect == null)
            {
                connect = new ErlTcpConnect(address, port);
                LIST.Add(connect);
            }
            return connect;
        }

        internal ErlTcpConnectFactory()
        {
        }
    }
}
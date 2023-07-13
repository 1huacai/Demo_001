public enum eYKNetEvent
{
    /// <summary>
    /// 网络连接已断开
    /// </summary>
	ConnectedDis,
    /// <summary>
    /// 准备连接网络
    /// </summary>
    ConnectStart,
    /// <summary>
    /// 连接socket成功
    /// </summary>
    ConnectSuccessPre,
    /// <summary>
    /// 连接完成
    /// </summary>
    ConnectSuccessComplete,
    /// <summary>
    /// 建立socket失败
    /// </summary>
    ConnectFailed,

    /// <summary>
    /// 网络重连完毕
    /// </summary>
    Reconnect,

    SocketReconnected,


    /// <summary>
    /// 发送/访问
    /// </summary>
    AccessMsg,
    /// <summary>
    /// 接收
    /// </summary>
	ReceiveMsg,
    ReceiveGlobalMsg,

    ReceiveMsgError,

    /// <summary>
    /// 心跳 echo
    /// </summary>
    Ping,

    /// <summary>
    /// 服务器返回了port为0的消息，服务器有错
    /// </summary>
    PortZero,
	
	    /// <summary>
    /// 通讯耗时
    /// </summary>
   CcommunicationCostTime,
}

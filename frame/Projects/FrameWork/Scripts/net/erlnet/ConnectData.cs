using System;
using System.Collections;
using System.Collections.Generic;

 public class ConnectData
{
    /// <summary>
    /// IP
    /// </summary>
    public string address;
    public int port;

    /// <summary>
    /// message can not be null.
    /// </summary>
    public ErlKVMessage message;
    public ReceiveFun receiveFun;
    public List<object> argus;
    public NetDef.SendType type;
    /// <summary>
    ///忽略排队限制
    /// </summary>
    public bool ignoreLimit = false;
    /// <summary>
    /// 记录访问，断线重连时 重发需要
    /// </summary>
    public ErlEntry erlEntry;


}
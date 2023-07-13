using System;
using System.Collections.Generic;

public class NetDef
{
    public enum ReceiveFunState 
    {
        Ok,
        Error,
        SystemError,
    }

    public enum SendType
    {
        SendNoReSend,   //ping包，重连后不再继续发送
        SendLogin,      //登录包，连接成功后发送登录包
        SendNormal,     //必须登录成功后，才能继续发送
    }

 
}

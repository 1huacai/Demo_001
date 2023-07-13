using CoreFrameWork;
using CoreFrameWork.TimeLua;
using System;
using System.Net;
using System.Net.Sockets;
using XCore.Event;
using XCore.IO;
using XCore.Utils;

namespace XCore.Net
{

    /// <summary>
    /// Tcp连接类
    /// </summary>
    public class TcpConnect : BaseEvent
    {
        public static bool DEBUG_LOG;
        /// <summary>
        /// 断开发送
        /// </summary>
        public static bool SEND_BREAK;
        /// <summary>
        /// 断开接收
        /// </summary>
        public static bool RECIVE_BREAK;

        public static short BREAK_SEND_MSGID = 0;
        public static short BREAK_RECVIE_MSGID = 0;
        /// <summary>
        /// 默认连接超时时长 秒级
        /// </summary>
        private const int connectTimeOut = 5;
        /// <summary>
        /// 默认ping间隔时间
        /// </summary>
        private const int defaultPingTime = 30;
        /// <summary>
        /// 套接字
        /// </summary>
        private Socket mSocket;
        /// <summary>
        /// 地址
        /// </summary>
        private string mAddress;
        /// <summary>
        /// 端口
        /// </summary>
        private int mPort;
        /// <summary>
        /// 连接超时时长
        /// </summary>
        private int mConnectTimeOut = connectTimeOut;
        /// <summary>
        /// ping间隔时间
        /// </summary>
        protected int pingIntervalTime = defaultPingTime;
        /// <summary>
        /// 上次ping时间
        /// </summary>
        protected long lastPingTime;
        /// <summary>
        /// 是否关闭
        /// </summary>
        private bool mIsClose;
        /// <summary>
        /// 是否连接成功
        /// </summary>
        private bool mIsConnect;
        /// <summary>
        /// 是否认证成功
        /// </summary>
        private bool mIsCertify;
        /// <summary>
        /// 报文头
        /// </summary>
        private readonly byte[] mHead = new byte[2];
        /// <summary>
        /// 已收报文长度
        /// </summary>
        private int mRecivedHeadLength;
        /// <summary>
        /// 报文正文长度
        /// </summary>
        private int mBodyLength;
        /// <summary>
        /// 报文正文
        /// </summary>
        protected ByteBuffer bodyBuffer;
        /// <summary>
        /// 地址
        /// </summary>
        public string Address
        {
            get { return mAddress; }
        }

        /// <summary>
        /// 端口
        /// </summary>
        public int Port
        {
            get { return mPort; }
        }

        /// <summary>
        /// 是否连接成功
        /// </summary>
        internal bool IsConnect
        {
            get { return mIsConnect; }
        }

        public bool IsCertify
        {
            get { return mIsCertify; }
        }

        /// <summary>
        /// 连接超时时长
        /// </summary>
        public int ConnectTimeOut
        {
            set { mConnectTimeOut = value; }
            get { return mConnectTimeOut; }
        }

        /// <summary>
        /// ping间隔时间
        /// </summary>
        public int PingIntervalTime
        {
            set { pingIntervalTime = value; }
            get { return pingIntervalTime; }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="port">端口</param>
        internal TcpConnect(string address, int port)
        {
            mAddress = address;
            mPort = port;
        }

        public bool CanConnect()
        {
            return mSocket == null;
        }

        private double connectTimeID = -1;
        private void RemoveConnectTime()
        {
            if(connectTimeID >= 0)
            {
                TimeCtrl.Instance.RemoveTime(connectTimeID);
                connectTimeID = -1;
            }
        }

        /// <summary>
        /// 开始连接
        /// </summary>
        public void BeginConnect()
        {
            if (mSocket != null)
            {
                Log.Warning("连接已存在，请关闭连接后再使用"+ "address"+ mAddress+ "port"+ mPort);
                return;
            }
            if (DEBUG_LOG)
            {
                Log.Debug("开始连接"+ "address"+ mAddress+ "port"+ mPort);
            }
            try
            {
                IPAddress[] ips = Dns.GetHostAddresses(mAddress);
                IPEndPoint ipe = new IPEndPoint(ips[0], mPort);
                if (DEBUG_LOG)
                {
                    Log.Debug("地址转换", "address"+ ipe.Address+ "port"+ ipe.Port);
                }
                mAddress = ipe.Address.ToString();
                mPort = ipe.Port;
                mIsClose = false;
                mSocket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                mSocket.SendBufferSize = 1024 * 1024;
                RemoveConnectTime();
                connectTimeID = TimeCtrl.Instance.SetCountDown(mConnectTimeOut, ConnectFail);
                //if (DEBUG_LOG)
                //{
                //    Log.Debug("添加超时timer"+ "mConnectTimerKey"+ mConnectTimerKey);
                //}
                mSocket.BeginConnect(ipe, ConnectCallBack, null);
            }
            catch (Exception e)
            {
                Log.Error(e+ "连接失败"+ "address"+ mAddress+ "port"+mPort);
                ConnectFail(0);
            }

        }

        private void ConnectCallBack(IAsyncResult ar)
        {
            if (mIsClose) return;
            RemoveConnectTime();
            if (mSocket.Connected)
            {
                if (DEBUG_LOG)
                {
                    Log.Debug("连接成功"+ "address"+ mAddress+ "port"+ mPort);
                }
                ConnectOk();
            }
            else
            {
                ConnectFail(10);
            }
        }

        private void ConnectOk()
        {
            if(RECIVE_BREAK || SEND_BREAK)
            {
                return;
            }
            mIsConnect = true;
            DispatchEventSingleAsyn(EventNumberUtils.CONNECT_OK, this);
            ReciveHead(0, mHead.Length);
        }

        private void ConnectFail(double tid)
        {
            Log.Warning("连接失败"+ "address"+ mAddress+ "port"+ mPort);
            DispatchEventSingleAsyn(EventNumberUtils.CONNECT_FAIL, this);
            //Close();
        }

        private void ReciveHead(int index, int length)
        {
            try
            {
                if (mIsClose)
                    return;
                //if (Log.Log.IsDebug())
                //{
                //    Log.Log.Debug("开始接收报文头", "address", mAddress, "port", mPort);
                //}
                mSocket.BeginReceive(mHead, index, length, SocketFlags.None, ReciveHeadCallBack, null);
            }
            catch (Exception e)
            {
                Log.Error(e+ "接收报文头报错"+ "address"+ mAddress+ "port"+ mPort+ "length"+ length+ "连接是否活动"+
                    mSocket.Connected);
            }

        }

        private void ReciveHeadCallBack(IAsyncResult ar)
        {
            try
            {
                if (mIsClose)
                    return;
                int length = mSocket.EndReceive(ar);
                //if (Log.Log.IsDebug())
                //{
                //    Log.Log.Debug("接收报文头", "address", mAddress, "port", mPort, "length", length);
                //}
                mRecivedHeadLength += length;
                if (mRecivedHeadLength < mHead.Length)
                {
                    ReciveHead(mRecivedHeadLength, mHead.Length - mRecivedHeadLength);
                }
                else if (mRecivedHeadLength == mHead.Length)
                {
                    mBodyLength = BytesUtils.ReadUnsignedShort(mHead);
                    if (DEBUG_LOG)
                    {
                        Log.Debug("接收报文头"+ "address"+ mAddress+ "port"+ mPort+ "数据"+ TextUtils.ToString(mHead)+ "报文正文长度"+ mBodyLength);
                    }
                    mRecivedHeadLength = 0;
                    if (bodyBuffer == null)
                    {
                        bodyBuffer = new ByteBuffer(mBodyLength);
                    }
                    else
                    {
                        bodyBuffer.Position = 0;
                        bodyBuffer.Top = 0;
                        bodyBuffer.SetCapacity(mBodyLength);
                    }
                    int top = bodyBuffer.Top;
                    ReciveBody(top, mBodyLength - top);
                }
                else
                {
                    Log.Warning("报文头解析数据异常"+ "address"+ mAddress+ "port"+ mPort+ "mRecivedHeadLength"+ mRecivedHeadLength+
                        "mHead.Length"+ mHead.Length);
                }
            }
            catch (Exception e)
            {
                Log.Error(e+ "接收报文头回调报错"+ "address"+ mAddress+ "port"+mPort+ "连接是否活动"+
                    mSocket.Connected);
            }
        }

        private void ReciveBody(int index, int length)
        {
            try
            {
                if (mIsClose)
                    return;
                if (DEBUG_LOG)
                {
                    Log.Debug("开始接收报文正文"+ "index"+ index+ "length"+ length);
                }
                mSocket.BeginReceive(bodyBuffer.GetArray(), index, length, SocketFlags.None, ReciveBodyCallBack, null);
            }
            catch (Exception e)
            {
                Log.Error(e+ "接收报文正文报错"+ "address"+ mAddress+ "port"+ mPort+ "连接是否活动"+
                    mSocket.Connected);
            }
        }

        private void ReciveBodyCallBack(IAsyncResult ar)
        {
            try
            {
                if (mIsClose)
                    return;
                int length = mSocket.EndReceive(ar);
                bodyBuffer.Top += length;
                if (DEBUG_LOG)
                {
                    Log.Debug("接收报文正文"+ "top"+ bodyBuffer.Top+ "length"+ length+ "capacity"+ bodyBuffer.Capacity());
                }
                if (bodyBuffer.Top == mBodyLength)
                {
                    byte[] bytes = bodyBuffer.GetArray();
                    //if (Log.Log.IsDebug() && DEBUG_LOG)
                    //{
                    //    Log.Log.Debug("接收数据", "address", mAddress, "port", mPort, "数据", TextUtils.ToString(bytes), "报文正文长度", mBodyLength);
                    //}
                    if (!IsCertify)
                    {
                        Certify(bytes);
                    }
                    else
                    {
                        UnPack();
                    }
                    mBodyLength = 0;
                    ReciveHead(0, mHead.Length);
                }
                else
                {
                    int top = bodyBuffer.Top;
                    ReciveBody(top, mBodyLength - top);
                }
            }
            catch (Exception e)
            {
                Log.Error(e+ "接收报文正文回调报错"+ "address"+ mAddress+ "port"+ mPort+ "连接是否活动"+
                    mSocket.Connected);
            }
        }

        protected virtual void Certify(byte[] bytes)
        {
            //if (Log.Log.IsInfo())
            //{
            //    Log.Log.Info("连接认证成功", "address", mAddress, "port", mPort);
            //}
            mIsCertify = true;
            DispatchEventSingleAsyn(EventNumberUtils.CONNECT_CERITY_OK, this);
        }

        protected virtual void UnPack()
        {

        }

        public void Send(byte[] bytes, int index, int length)
        {
            if (mSocket == null || !mSocket.Connected || mIsClose)
            {
                return;
            }
            if (DEBUG_LOG)
            {
                Log.Debug("发送数据"+ TextUtils.ToString(bytes, index, length)+ "长度"+ length+ "连接是否活动"+ mSocket.Connected);
            }

            try
            {
                mSocket.Send(bytes, index, length, SocketFlags.None);
            }
            catch (Exception e)
            {
                Log.Error("TcpConnect.Send: "+ e);
            }
        }

        protected virtual void Ping()
        {
            if (IsCertify)
            {
                long temp = DateTime.Now.Ticks / 10000000;
                if (pingIntervalTime == 0)
                {
                    lastPingTime = temp;
                    return;
                }
                if (temp - lastPingTime >= pingIntervalTime)
                {
                    lastPingTime = temp;
                    SendPing();
                }
            }
        }

        protected virtual void SendPing()
        {

        }

        public override void Update()
        {
            base.Update();
            //Ping();
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        internal virtual void Close()
        {
            if (mIsClose)
                return;
            if (DEBUG_LOG)
            {
                Log.Debug("关闭连接"+ mAddress+ mPort);
            }
            mIsClose = true;
            mIsConnect = false;
            mIsCertify = false;
            if (mSocket != null)
            {
                try
                {
                    mSocket.Shutdown(SocketShutdown.Both);
                }
                catch
                {
                    // ignored
                }
                mSocket.Close();
                mSocket = null;
            }
            mRecivedHeadLength = 0;
            mBodyLength = 0;
            bodyBuffer = null;
            DispatchEventSingleAsyn(EventNumberUtils.CONNECT_CLOSE, this);
            Destroy();
        }
    }
}

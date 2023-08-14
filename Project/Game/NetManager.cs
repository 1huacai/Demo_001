using Project;
using UnityEngine;

namespace Demo
{
    public class NetManager
    {
        private static NetManager _instance;
        
        public static NetManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new NetManager();
                return _instance;
            }
        }

        private bool server_logined = false;
        private MsgHandler _msgHandler;
        
        public bool Server_Logined
        {
            get { return server_logined; }  
            set { server_logined = value; }
        }

        private bool _multiPlayer;//多人游戏标签

        public bool Multiplayer
        {
            set { _multiPlayer = value; }
            get { return _multiPlayer; }
        }
        
        
        public void Init()
        {
            NetCore.Init();
            NetSender.Init();
            NetReceiver.Init();
            NetCore.enabled = true;
            _msgHandler = new MsgHandler();
            _msgHandler.InitHandler();
        }

        public void Update()
        {
            NetCore.Dispatch();
        }
        
        
        //游戏登陆
        public void ConnetServer ()
        {
            if (NetCore.connected == false)
            {
                Debug.LogError("开始链接");
                NetManager.Instance.Server_Logined = false;
                NetCore.Connect(ConstValues.serverIp, ConstValues.serverPort, () =>
                {
                    Debug.LogError("connect server success");
                    //auth请求
                    var req = new C2S_SprotoType.auth.request()
                    {
                        imei = SystemInfo.deviceUniqueIdentifier,
                        version = "20230804"
                    };
                    NetSender.Send<C2S_Protocol.auth>(req,(rsp1 =>
                    {
                        var resp = rsp1 as C2S_SprotoType.auth.response;
                        Debug.LogError( $"auth response :e = {resp.e},rid={resp.rid},server_time={resp.server_time}");
                        
                        //login
                        var loginReq = new C2S_SprotoType.login.request()
                        {
                            rid = resp.rid
                        };
                        NetSender.Send<C2S_Protocol.login>(loginReq,(rsp2 =>
                        {
                            var data = rsp2 as C2S_SprotoType.login.response;
                            Debug.LogError($"login response:{data.e}--rename - {data.rname}--render - {data.render}");
                        }));

                    }));
                });
            }
        }
        
        public void GameBattle()
        {
            if (NetCore.connected == false)
            {
                server_logined = false;
                NetCore.Connect(ConstValues.serverIp, ConstValues.serverPort, () =>
                {
                    Debug.Log("connect server success");
                });
            }
            if (server_logined)
                MatchReq();
            else
                GameBattle();
        }
        
        //向服务器发起匹配请求
        private void MatchReq()
        {
            Debug.LogError("开始发送匹配请求");
            
        }
        
    }
}
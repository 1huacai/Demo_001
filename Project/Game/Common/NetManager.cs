using System;
using C2S_SprotoType;
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
                Server_Logined = false;
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
                            Server_Logined = true;
                        }));

                    }));
                });
            }
        }
        
        //多人战斗开始
        public void GameBattle(Action callback)
        {
            if (NetCore.connected == false)
            {
                // ConnetServer();
                Server_Logined = false;
                NetCore.Connect(ConstValues.serverIp, ConstValues.serverPort, () =>
                {
                    Server_Logined = true;
                });
            }
            
            if (server_logined)
                MatchStartReq(callback);
            else
                GameBattle(callback);
        }
        
        //向服务器发起匹配请求
        public void MatchStartReq(Action callBack = null)
        {
            Debug.LogError("开始发送匹配请求");
            var matchStartrtRequest = new C2S_SprotoType.match_start.request();
            NetSender.Send<C2S_Protocol.match_start>(matchStartrtRequest, (rsp) =>
            {
                var data = rsp as C2S_SprotoType.match_start.response;
                if (data.e == 0)
                {
                    Debug.LogError("开始匹配计时");
                    Multiplayer = true;
                    callBack?.Invoke();
                }
            });
        }
        
        //取消匹配请求
        public void MatchCancelReq(Action callBack = null)
        {
            Debug.LogError("开始发送取消匹配请求");
            var matchCancelRequest = new C2S_SprotoType.match_cancel.request();
            NetSender.Send<C2S_Protocol.match_cancel>(matchCancelRequest,(rsp2 =>
            {
                var data2 = rsp2 as C2S_SprotoType.match_cancel.response;
                if (data2.e == 0)
                {
                    Debug.LogError("取消匹配");
                    Multiplayer = false;
                    callBack?.Invoke();
                }
            }));
        }
        
        //交换方块请求
        public void GameSwapReq(int frame,Block block_1,Block block_2,Action callBack)
        {
            block_info blockInfo_1 = new block_info()
            {
                row = block_1.Row, col = block_1.Col, shape = (int) block_1.Shape, state = (int) block_1.State,frame = 0
            };
            
            block_info blockInfo_2 = new block_info()
            {
                row = block_2.Row, col = block_2.Col, shape = (int) block_2.Shape, state = (int) block_2.State,frame = 0
            };

            var req = new C2S_SprotoType.game_swap.request()
            {
                frame = frame,
                block1 = blockInfo_1,
                block2 = blockInfo_2
            };
            
            NetSender.Send<C2S_Protocol.game_swap>(req,(rsp =>
            {
                var data = rsp as C2S_SprotoType.game_swap.response;
                if (data.e == 0)
                {
                    callBack?.Invoke();
                }
            }));
        }

        public void GameRaiseReq(int frame,Action callBack)
        {
            var req = new C2S_SprotoType.game_raise.request()
            {
                frame = frame
            };
            NetSender.Send<C2S_Protocol.game_raise>(req,(rsp =>
            {
                var data = rsp as C2S_SprotoType.game_raise.response;
                if (data.e == 0)
                {
                    callBack?.Invoke();
                }
            }));
        }
        
        
        
        
    }
}
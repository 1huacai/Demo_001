using System;
using System.Collections.Generic;
using System.Text;
using C2S_SprotoType;
using Project;
using UnityEngine;
using System.IO;

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
                            SelfGameController.Inst.selfUserName = data.rname;
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
            var matchStartrtRequest = new C2S_SprotoType.matching_start.request();
            NetSender.Send<C2S_Protocol.matching_start>(matchStartrtRequest, (rsp) =>
            {
                var data = rsp as C2S_SprotoType.matching_start.response;
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
            var matchCancelRequest = new C2S_SprotoType.matching_cancel.request();
            NetSender.Send<C2S_Protocol.matching_cancel>(matchCancelRequest,(rsp2 =>
            {
                var data2 = rsp2 as C2S_SprotoType.matching_cancel.response;
                if (data2.e == 0)
                {
                    Debug.LogError("取消匹配");
                    Multiplayer = false;
                    callBack?.Invoke();
                }
            }));
        }
        
        //交换方块请求
        public void GameSwapReq(int frame,Block block_1,Block block_2,bool isSelf,Action<Block,Block,bool> callBack)
        {
            //客户端操作不能等服务器回复，不然会卡顿
            callBack?.Invoke(block_1,block_2,isSelf);
            
            //交换请求
            var block1 = block_1;
            var block2 = block_2;
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
                    Debug.LogError($"{block1.Row}-{block1.Col} 和 {block2.Row}-{block2.Col} 交换验证通过!!!");
                }
            }));
        }
        
        /// <summary>
        /// 提升一行操作，type（1是手动提升，2是自动上升）
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="type"></param>
        /// <param name="callBack"></param>
        public void GameRaiseReq(int frame,int type,Action callBack)
        {
            Debug.LogError($"提升一行请求：操作类型-{type}");
            callBack?.Invoke();
            
            var req = new C2S_SprotoType.game_raise.request()
            {
                frame = frame,
                type = type
            };
            NetSender.Send<C2S_Protocol.game_raise>(req);
        }
        
       /// <summary>
       /// 匹配消除请求
       /// </summary>
       /// <param name="frame"></param>
       /// <param name="blocks"></param>
       /// <param name="metalCount">金属块数量</param>
       /// <param name="chainCount">chain数量</param>
       /// <param name="callBack"></param>
        public void GameMatched(int frame, List<Block> blocks,int metalCount,int chainCount,Action callBack)
        {
            callBack?.Invoke();
            
            List<block_info> blockInfos = new List<block_info>();
            foreach (var block in blocks)
            {
                block_info info = new block_info()
                {
                    row = block.Row,
                    col = block.Col,
                    shape = (int)block.Shape,
                    state = (int)block.State,
                    frame = frame
                };
                blockInfos.Add(info);
            }

            var req = new C2S_SprotoType.game_matched.request()
            {
                frame = frame,
                matched_blocks = blockInfos,
                metal_count = metalCount,
                chain_count = chainCount
            };
            
            NetSender.Send<C2S_Protocol.game_matched>(req,(rsp =>
            {
                var data = rsp as C2S_SprotoType.game_matched.response;
                if (data.e == 0)
                {
                    Debug.LogError("消除请求验证通过");
                }
            }));
        }
       
        private StringBuilder _builder = new StringBuilder();
        //更新本地棋子的日志
        public void UpdateWebLogs(Block[,] blocks)
        {
            if (!SelfGameController.Inst.gameStart)
            {
                return;
            }
            
            _builder.Clear();
            _builder.Append($"TotalFrame:{TimerMgr._Instance.Frame}\n");
            
            foreach (var block in blocks)
            {
                if (block != null)
                {
                    string blockLog = $"{block.CurStateFrame-(int)block.State}-{(int) block.Shape}-{block.Row}-{block.Col}\n";
                    _builder.Append(blockLog);
                }
            }
            
            _builder.Append("\n");
            if (!Directory.Exists(Application.streamingAssetsPath + "/Log"))
            {
                Directory.CreateDirectory(Application.streamingAssetsPath + "/Log");
            }
            
            File.AppendAllText(Application.streamingAssetsPath + "/Log/Block_Log.txt", _builder.ToString());
            
        }
        
    }
}
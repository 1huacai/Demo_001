using UnityEngine;
namespace Demo
{
    public class MsgHandler
    {   
        //初始化服务器回复消息处理
        public void InitHandler()
        {
            #region 匹配
            //匹配成功
            NetReceiver.AddHandler<S2C_Protocol.matching_success>((data) =>
            {
                Debug.LogError("========= match_success");
                var requst = data as S2C_SprotoType.matching_success.request;
                var players = requst.players;
                UIManager.Inst.GetUI<GameView>(UIDef.GameView).SetMultiplayerInfo(players[0].rname,players[1].rname);
                // Debug.LogError("玩家数量:"+players.Count);
                Debug.LogError($"对战玩家{players[0].rname} vs {players[1].rname}");
                return null;
            });
            
            //匹配超时
            NetReceiver.AddHandler<S2C_Protocol.matching_timeout>((data) =>
            {
                Debug.LogError("========= match_timeout");
                return null;
            });
            
            //匹配异常
            NetReceiver.AddHandler<S2C_Protocol.matching_error>((data) =>
            {
                Debug.LogError("========= match_error");
                return null;
            });
            #endregion

            #region 比赛
            //推送比赛信息
            NetReceiver.AddHandler<S2C_Protocol.game_info>((data) =>
            {
                Debug.LogError("========= game_info");
                return null;
            });
            
            //游戏准备
            NetReceiver.AddHandler<S2C_Protocol.game_ready>((data) =>
            {
                Debug.LogError("========= game_ready");
                
                var req = new C2S_SprotoType.game_ready.request();
                NetSender.Send<C2S_Protocol.game_ready>(req);

                return null;
            });
            
            //游戏开始
            NetReceiver.AddHandler<S2C_Protocol.game_start>((data) =>
            {
                Debug.LogError("========= game_start");
                SelfGameController.Inst.gameStart = true;
                return null;
            });
            
            //数据同步：交换方块
            NetReceiver.AddHandler<S2C_Protocol.game_swap>((data) =>
            {
                Debug.LogError("========= game_swap");
                return null;
            });
            
            //数据同步：提升一行
            NetReceiver.AddHandler<S2C_Protocol.game_raise>((data) =>
            {
                Debug.LogError("========= game_raise");
                return null;
            });
            
            //数据同步：匹配消除
            NetReceiver.AddHandler<S2C_Protocol.game_matched>((data) =>
            {
                Debug.LogError("========= game_matched");
                return null;
            });
            
            //数据同步：生成新的一行方块
            NetReceiver.AddHandler<S2C_Protocol.game_new_row>((data) =>
            {
                Debug.LogError("========= game_new_row");
                return null;
            });
            
            //生成方块的buffer,第一次游戏没开始时做初始化，然后后面新来的再往里面加
            NetReceiver.AddHandler<S2C_Protocol.game_block_buffer>((data) =>
            {
                Debug.LogError("========= game_block_buffer");
                var request = data as S2C_SprotoType.game_block_buffer.request;
                Debug.LogError($"InitBlockBuffer-{request.buffer}--{request.buffer.Length}");
                //TODO 从这里添加初始化blocks
                if (!SelfGameController.Inst.gameStart)
                {
                    //游戏未开始获得初始化blockbuffer
                    SelfGameController.Inst.blockBufferWithNet = request.buffer;
                    UIManager.Inst.GetUI<LoginView>(UIDef.LoginView).GameReadyEnterGame();
                }
                else
                {
                    //游戏开始新的blockbuffer加入末尾
                    SelfGameController.Inst.blockBufferWithNet += request.buffer;
                }
                
                return null;
            });
            
            //生成压力块的buffer
            NetReceiver.AddHandler<S2C_Protocol.game_garbage_buffer>((data) =>
            {
                Debug.LogError("========= game_garbage_buffer");
                return null;
            });
            
            //数据同步：回退
            NetReceiver.AddHandler<S2C_Protocol.game_rollback>((data) =>
            {
                Debug.LogError("========= game_rollback");
                return null;
            });
            
            //比赛结束
            NetReceiver.AddHandler<S2C_Protocol.game_over>((data) =>
            {
                Debug.LogError("========= game_over");
                return null;
            });
            
            #endregion
        }
    }
}
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
                UIManager.Inst.GetUI<LoginView>(UIDef.LoginView).GameReadyEnterGame();
                
                var request = data as S2C_SprotoType.game_ready.request;
                var blockDatas = request.blocks;
                Debug.LogError("初始化block数量--" + blockDatas.Count);
                
                return null;
            });
            
            //游戏开始
            NetReceiver.AddHandler<S2C_Protocol.game_start>((data) =>
            {
                Debug.LogError("========= game_start");
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
            
            //生成方块的buffer
            NetReceiver.AddHandler<S2C_Protocol.game_block_buffer>((data) =>
            {
                Debug.LogError("========= game_block_buffer");
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
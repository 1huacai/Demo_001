using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrameWork
{
    public enum GameEvent
    {
        GameStart,
        GameTreeNodeChange,
        BeginOpenStage,
        BattleStart,
        OnUIOpened,
        OnUIClosed,
        OnUIHide,
        /// <summary>
        /// 禁止主场景渲染动画脚本状态变化
        /// </summary>
        OnGraphicDrawObstructionChange,

        SubCameraInfoChange,
    }
}

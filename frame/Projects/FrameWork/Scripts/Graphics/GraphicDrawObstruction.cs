using System.Collections.Generic;
using UnityEngine;
using FrameWork.App;
using FrameWork.GUI.AorUI;
using FrameWork.Manager;

namespace FrameWork.Graphics
{

    /// <summary>
    /// 图形绘制阻挠器,场景中只要有任何一个阻挠器,图形管理器就不再更新游戏画面(排除UI)
    /// </summary>
    [DisallowMultipleComponent]
    public class GraphicDrawObstruction : MonoSwitch
    {

        static List<GraphicDrawObstruction> obstructions = new List<GraphicDrawObstruction>();

        void check()
        {
            bool isStopRender = obstructions.Count > 0 ? true : false;
            SingletonManager.GetManager<GraphicsManager>().StopRender = isStopRender;
            ApplicationCore.Instance.GlobalEvent.DispatchEvent(GameEvent.OnGraphicDrawObstructionChange, isStopRender);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            obstructions.Remove(this);
            check();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            obstructions.Add(this);
            check();
        }
    }
}

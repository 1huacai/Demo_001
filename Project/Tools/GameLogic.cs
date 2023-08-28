using System;
using System.Collections;
using UnityEngine;

namespace Demo.Tools
{
    public class GameLogic : MonoBehaviour
    {
        private void Awake()
        {
        }

        private void Start()
        {
        }

        private void Update()
        {
            TimerMgr._Instance.Update();
            NetManager.Instance.Update();
        }

        private void FixedUpdate()
        {
            SelfGameController.Inst.FiexdUpdate();
            PressureUnlocking();
        }

        private void LateUpdate()
        {
            SelfGameController.Inst.LateUpdate();
        }

        private void PressureUnlocking()
        {
            if (!SelfGameController.Inst.PreussUnlocking)
            {
                if (SelfGameController.Inst.unlockPressBlocks.Count > 0)
                {
                    SelfGameController.Inst.PreussUnlocking = true;
                    SelfGameController.Inst.unlockPressBlocks.Sort(((pressure_1, pressure_2) =>
                    {
                        return pressure_2.OriginCol - pressure_1.OriginCol;
                    }));

                    SelfGameController.Inst.unlockPressBlocks.Sort(((pressure_1, pressure_2) =>
                    {
                        return pressure_1.Row - pressure_2.Row;
                    }));

                    UnlockPressBlocks();
                }
            }
        }

        private float lockAnimTime = 0.57f; //当个小压力快的动画时间

        private void UnlockPressBlocks()
        {
            // Debug.LogError("待解锁的压力块个数----" + GameManger.Inst.unlockPressBlocks.Count);
            SelfGameController.Inst.genBlocksByPressure.Clear();
            var targetPressureBlock = SelfGameController.Inst.unlockPressBlocks[0];
            targetPressureBlock.IsUnlocking = true;
        }
    }
}
using System;
using System.Collections;
using UnityEngine;

namespace Demo.Tools
{
    public class GameLogic:MonoBehaviour
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
                    //TODO 把解锁的压力块集合排序
                    SelfGameController.Inst.unlockPressBlocks.Sort(((pressure_1, pressure_2) =>
                    {
                        return pressure_2.OriginCol - pressure_1.OriginCol;
                    }));

                    SelfGameController.Inst.unlockPressBlocks.Sort(((pressure_1, pressure_2) =>
                    {
                        return pressure_1.Row - pressure_2.Row;
                    }));

                    SelfGameController.Inst.PreussUnlocking = true;
                    StartCoroutine(UnlockPressBlocks());
                }
            }
        }

        private float lockAnimTime = 0.57f;//当个小压力快的动画时间
        private IEnumerator UnlockPressBlocks()
        {
            // Debug.LogError("待解锁的压力块个数----" + GameManger.Inst.unlockPressBlocks.Count);
            
            for (int i = 0; i < SelfGameController.Inst.unlockPressBlocks.Count; i++)
            {
                var targetPressureBlock = SelfGameController.Inst.unlockPressBlocks[i];
                targetPressureBlock.IsUnlocking = true;
                float waittime = lockAnimTime * (targetPressureBlock.IsChain ? 6 : targetPressureBlock.SingleBlocks.Count);
                yield return new WaitForSeconds(waittime);
            }

            for (int i = 0; i < SelfGameController.Inst.unlockPressBlocks.Count; i++)
            {
                var pressureBlock = SelfGameController.Inst.unlockPressBlocks[i];
                var genBlocks = pressureBlock.genBlocks;
                
                foreach (var block in genBlocks)
                {
                    block.GenByGarbage = false;
                }

                if (pressureBlock.IsChain)
                {
                    genBlocks.Clear();
                    Debug.LogError("2_C_"+pressureBlock.transform.Find("RbPart").childCount);
                    if (pressureBlock.transform.Find("RbPart").childCount > 0)
                    {
                        //大块还没消除干净防止被误删除
                        SelfGameController.Inst.unlockPressBlocks.Remove(pressureBlock);
                    }
                }
                else
                {
                    SelfGameController.Inst.pressureBlocks.Remove(pressureBlock);
                }
                
            }
            
            SelfGameController.Inst.PreussUnlocking = false;
            SelfGameController.Inst.BoardStopRise = false;
            for (int i = 0; i < SelfGameController.Inst.unlockPressBlocks.Count; i++)
            {
                Destroy(SelfGameController.Inst.unlockPressBlocks[i].gameObject);
            }
            SelfGameController.Inst.unlockPressBlocks.Clear();
        }
    }
}
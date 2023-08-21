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
            // SelfGameController.Inst.FiexdUpdate();
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
                    // GameManger.Inst.unlockPressBlocks.Sort(((block, pressureBlock) => ));
                    
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
                float waittime = lockAnimTime * targetPressureBlock.SingleBlocks.Count;
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
                
                SelfGameController.Inst.pressureBlocks.Remove(pressureBlock);
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
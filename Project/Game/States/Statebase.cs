using Project;
using UnityEngine;

namespace Demo
{
    public class Statebase
    {
        protected SelfGameController SelfGameController;

        //对应的计时器id，唯一
        protected int timerID = 0;

        public Statebase(SelfGameController controller)
        {
            SelfGameController = controller;
        }

        public virtual void OnBlockOperation(int row, int col, BlockOperation operation)
        {
        }

        public virtual void OnDestroy()
        {
            SelfGameController = null;
        }


        #region 状态逻辑更新

        public virtual void Enter(Block block)
        {
            
        }

        public virtual void Update(Block block)
        {
            if (block.Type == BlockType.None)
                return;
           
        }

        public virtual void Exit(Block block)
        {
           
        }

        public virtual void Enter(PressureBlock pressureBlock)
        {
            
        }

        public virtual void Update(PressureBlock pressureBlock)
        {
            
        }
        
        public virtual void Exit(PressureBlock block)
        {
           
        }
        
        #endregion
    }
}
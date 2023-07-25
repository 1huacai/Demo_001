using Project;

namespace Demo
{
    public class Statebase
    {
        protected GameManger _gameManger;
        //对应的计时器id，唯一
        protected int timerID = 0;
        public Statebase(GameManger manger)
        {
            _gameManger = manger;
        }

        public virtual void OnBlockOperation(int row, int col, BlockOperation operation)
        {
            
        }

        public virtual void OnDestroy()
        {
            _gameManger = null;
        }


        #region 状态逻辑更新

        public virtual void Enter(Block block)
        {
            
        }

        public virtual void Update(Block block)
        {
            
        }

        public virtual void Exit(Block block)
        {
            if(timerID != 0)
                TimerMgr._Instance.Unschedule(timerID);
        }

        #endregion
        
    }
}